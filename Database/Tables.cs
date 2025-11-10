using System.Reflection;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Manages tables and the state of the cluster.
/// </summary>
public static class Tables
{
    /// <summary>
    /// The currently loaded tables.
    /// </summary>
    public static Dictionary<string,AbstractTable> Dictionary { get; set; } = [];

    /// <summary>
    /// This node's random node ID in the cluster.
    /// </summary>
    public static readonly string NodeId = DateTime.UtcNow.Ticks + Parsers.RandomString(32);
    
    private static ClusterNode? _Self = null;
    /// <summary>
    /// The cluster node that has been identified as this program instance.
    /// </summary>
    public static ClusterNode Self
    {
        get => _Self ?? throw new Exception("No cluster node was identified as this server instance.");
        internal set => _Self = value;
    }

    /// <summary>
    /// Returns the version of the given type.
    /// </summary>
    internal static Version GetTypeVersion(Type type)
    {
        var assembly = Assembly.GetAssembly(type);
        return assembly?.GetName().Version ?? new Version("1.0.0.0");
    }
    
    /// <summary>
    /// Checks all tables for issues and attempts to fix any issues.
    /// </summary>
    public static void CheckAndFixAll()
    {
        foreach (var t in Dictionary)
            t.Value.CheckAndFix();
    }
    
    /// <summary>
    /// Backs up all tables to the given backup ID, based on the previous backup if a previous ID has been set.
    /// </summary>
    public static void BackupAll(string id, string? previousId)
    {
        var stateDir = $"{Server.Config.Backup.Directory}{id}/Database";
        Directory.CreateDirectory(stateDir);
        var state = previousId != null ? Serialization.Deserialize<BackupState>(File.ReadAllBytes($"{Server.Config.Backup.Directory}{previousId}/Database/State.json")) ?? new() : new();
        
        // list tables
        HashSet<string> currentTables = [];
        foreach (var tableKV in Dictionary)
        {
            var tableDir = $"{stateDir}/{tableKV.Key.ToBase64PathSafe()}";
            currentTables.Add(tableKV.Key);
            var tableState = state.Tables.GetValueOrAdd(tableKV.Key, () => new());
            
            // list entries
            HashSet<string> currentEntries = [];
            bool entriesDirCreated = false;
            var entriesDir = $"{tableDir}/Entries";
            foreach (var entry in tableKV.Value.ListAbstractEntries())
            {
                currentEntries.Add(entry.Id);
                var entryState = tableState.Entries.GetValueOrDefault(entry.Id);
                entry.Lock.EnterReadLock();
                try
                {
                    if (entryState != null && entryState.Timestamp == entry.EntryInfo.Timestamp)
                        continue;
                    
                    // entry changed
                    if (!entriesDirCreated)
                    {
                        Directory.CreateDirectory(entriesDir);
                        entriesDirCreated = true;
                    }
                    if (entryState == null)
                    {
                        entryState = new(id, entry.EntryInfo.Timestamp);
                        tableState.Entries[entry.Id] = entryState;
                    }
                    else
                    {
                        entryState.Timestamp = entry.EntryInfo.Timestamp;
                        entryState.Origin = id;
                    }
                    File.Copy(entry.Path, $"{entriesDir}/{entry.Id.ToBase64PathSafe()}.json");
                    
                    // list files
                    HashSet<string> currentFiles = [];
                    bool filesDirCreated = false;
                    var filesDir = $"{tableDir}/Files/{entry.Id.ToBase64PathSafe()}";
                    foreach (var fileKV in entry.EntryInfo.Files)
                    {
                        currentFiles.Add(fileKV.Key);
                        var fileState = entryState.Files.GetValueOrDefault(fileKV.Key);
                        if (fileState != null && fileState.Timestamp == fileKV.Value.Timestamp)
                            continue;
                        
                        // file changed
                        if (!filesDirCreated)
                        {
                            Directory.CreateDirectory(filesDir);
                            filesDirCreated = true;
                        }
                        if (fileState == null)
                        {
                            fileState = new(id, fileKV.Value.Timestamp);
                            entryState.Files[fileKV.Key] = fileState;
                        }
                        else
                        {
                            fileState.Timestamp = fileKV.Value.Timestamp;
                            fileState.Origin = id;
                        }
                        File.Copy(entry.GetFilePath(fileKV.Key), $"{filesDir}/{fileKV.Key.ToBase64PathSafe()}");
                    }
                    
                    // mark missing files as deleted
                    foreach (var fileId in entryState.Files.Keys)
                        if (!currentFiles.Contains(fileId))
                            entryState.Files.Remove(fileId);
                }
                finally
                {
                    entry.Lock.ExitReadLock();
                }
            }
            
            // mark missing entries as deleted
            foreach (var entryId in tableState.Entries.Keys.ToList())
                if (!currentEntries.Contains(entryId))
                    tableState.Entries.Remove(entryId);
        }
        
        // mark missing tables as deleted
        foreach (var tableName in state.Tables.Keys.ToList())
            if (!currentTables.Contains(tableName))
                state.Tables.Remove(tableName);
        
        File.WriteAllBytes($"{stateDir}/State.json", Serialization.Serialize(state));
    }
    
    /// <summary>
    /// Restores all tables from the backup with the given ID.
    /// </summary>
    public static void RestoreAll(string id)
    {
        var state = Serialization.Deserialize<BackupState>(File.ReadAllBytes($"{Server.Config.Backup.Directory}{id}/Database/State.json")) ?? throw new Exception("Failed to deserialize backup state");
        
        // list tables
        foreach (var tableKV in state.Tables)
        {
            var tableDir = $"../Database/{tableKV.Key.ToBase64PathSafe()}";
            var table = Dictionary.GetValueOrDefault(tableKV.Key);
            if (table == null)
            {
                // restore table in the raw file system
                Directory.Delete(tableDir, true);
                Directory.CreateDirectory($"{tableDir}/Entries");
                Directory.CreateDirectory($"{tableDir}/Files");
                foreach (var entryKV in tableKV.Value.Entries)
                {
                    File.Copy($"{Server.Config.Backup.Directory}{entryKV.Value.Origin}/Database/{tableKV.Key.ToBase64PathSafe()}/Entries/{entryKV.Key.ToBase64PathSafe()}.json", $"{tableDir}/Entries/{entryKV.Key.ToBase64PathSafe()}.json");
                    bool filesDirCreated = false;
                    var filesDir = $"{tableDir}/Files/{entryKV.Key.ToBase64PathSafe()}";
                    foreach (var fileKV in entryKV.Value.Files)
                    {
                        if (!filesDirCreated)
                        {
                            Directory.CreateDirectory(filesDir);
                            filesDirCreated = true;
                        }
                        File.Copy($"{Server.Config.Backup.Directory}{fileKV.Value.Origin}/Database/{tableKV.Key.ToBase64PathSafe()}/Files/{entryKV.Key.ToBase64PathSafe()}/{fileKV.Key.ToBase64PathSafe()}", $"{filesDir}/{fileKV.Key.ToBase64PathSafe()}");
                    }
                }
            }
            else
            {
                // restore table efficiently
                foreach (var entryKV in tableKV.Value.Entries)
                {
                    if (table.TryGetAbstractEntry(entryKV.Key, out var entry))
                        entry.Lock.EnterWriteLock();
                    else
                        entry = table.CreateAndLockBlankAbstractEntry(entryKV.Key);
                    
                    try
                    {
                        if (entry.EntryInfo.Timestamp == entryKV.Value.Timestamp)
                            continue;
                    
                        bool filesDirCreated = false;
                        var filesDir = $"{tableDir}/Files/{entryKV.Key.ToBase64PathSafe()}";
                        foreach (var fileKV in entryKV.Value.Files)
                        {
                            if (entry.EntryInfo.Files.TryGetValue(fileKV.Key, out var fileData) && fileData.Timestamp == entryKV.Value.Timestamp)
                                continue;
                            
                            if (!filesDirCreated)
                            {
                                Directory.CreateDirectory(filesDir);
                                filesDirCreated = true;
                            }
                            if (File.Exists($"{filesDir}/{fileKV.Key.ToBase64PathSafe()}"))
                                File.Delete($"{filesDir}/{fileKV.Key.ToBase64PathSafe()}");
                            File.Copy($"{Server.Config.Backup.Directory}{fileKV.Value.Origin}/Database/{tableKV.Key.ToBase64PathSafe()}/Files/{entryKV.Key.ToBase64PathSafe()}/{fileKV.Key.ToBase64PathSafe()}", $"{filesDir}/{fileKV.Key.ToBase64PathSafe()}");
                        }
                        
                        foreach (var fileId in entry.EntryInfo.Files.Keys.ToList())
                            if (!entryKV.Value.Files.ContainsKey(fileId))
                                File.Delete($"{filesDir}/{fileId.ToBase64PathSafe()}");
                        
                        entry.SetBytes(File.ReadAllBytes($"{Server.Config.Backup.Directory}{entryKV.Value.Origin}/Database/{tableKV.Key.ToBase64PathSafe()}/Entries/{entryKV.Key.ToBase64PathSafe()}.json"));
                    }
                    finally
                    {
                        entry.Lock.ExitWriteLock();
                    }
                }
                
                // delete remaining entries
                foreach (var entry in table.ListAbstractEntries())
                    if (!tableKV.Value.Entries.ContainsKey(entry.Id))
                        table.Delete(entry.Id);
            }
        }
        
        // delete remaining tables
        foreach (var tableName in Dictionary.Keys.ToList())
            if (!state.Tables.ContainsKey(tableName))
            {
                Dictionary.Remove(tableName);
                Directory.Delete($"../Database/{tableName.ToBase64PathSafe()}", true);
            }
    }

    /// <summary>
    /// Attempts to mark this program instance in the database cluster.
    /// </summary>
    public static async Task MarkSelf()
    {
        if (Server.Config.Database.Cluster.Count == 0)
            return;

        if (Server.GetCertificate(Server.Config.Database.CertificateDomain) == null)
        {
            Console.WriteLine($"No certificate found for database (domain \"{Server.Config.Database.CertificateDomain}\"), database will not connect to the cluster.");
            Server.Config.Database.Cluster = [];
            return;
        }

        await Task.WhenAll(Server.Config.Database.Cluster.Select(node => node.MarkSelf()));
        
        if (_Self == null)
        {
            Console.WriteLine("Could not identify any node as this process, database will not connect to the cluster.");
            Server.Config.Database.Cluster = [];
        }
    }
    
    /// <summary>
    /// Starts connection monitors for all cluster nodes.
    /// </summary>
    public static void StartMonitoringConnections()
    {
        foreach (var node in Server.Config.Database.Cluster.Where(node => node.IsReachable))
            _ = node.MonitorConnection();
    }
    
    /// <summary>
    /// Stops the connection monitors for all cluster nodes.
    /// </summary>
    public static void StopMonitoringConnections()
    {
        foreach (var node in Server.Config.Database.Cluster.Where(node => node.IsReachable))
            node.StopMonitoringConnection();
    }
}