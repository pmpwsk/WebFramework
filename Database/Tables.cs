using System.Diagnostics.CodeAnalysis;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

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
    /// Checks all tables for issues and attempts to fix any issues.
    /// </summary>
    public static async Task CheckAndFixAllAsync()
    {
        foreach (var t in Dictionary)
            await t.Value.CheckAndFixAsync();
    }
    
    /// <summary>
    /// Attempts to find the loaded table with the given name and table type.
    /// </summary>
    public static T? TryGetTable<T>(string name) where T : AbstractTable
        => Dictionary.TryGetValue(name, out var abstractTable) && abstractTable is T typedTable ? typedTable : null;
    
    /// <summary>
    /// Attempts to find the loaded table with the given name and table type.
    /// </summary>
    public static bool TryGetTable<T>(string name, [MaybeNullWhen(false)] out T table) where T : AbstractTable
    {
        table = TryGetTable<T>(name);
        return table != null;
    }
    
    /// <summary>
    /// Backs up all tables to the given backup ID, based on the previous backup if a previous ID has been set.
    /// </summary>
    public static async Task BackupAllAsync(string id, string? previousId)
    {
        var stateDir = $"{Server.Config.Backup.Directory}{id}/Database";
        Directory.CreateDirectory(stateDir);
        var state = previousId != null ? Serializers.DataContractJson.DeserializeNullable<BackupState>(await File.ReadAllBytesAsync($"{Server.Config.Backup.Directory}{previousId}/Database/State.json")) ?? new() : new();
        
        // list tables
        HashSet<string> currentTables = [];
        foreach (var (tableName, table) in Dictionary)
        {
            var tableDir = $"{stateDir}/{tableName.ToBase64PathSafe()}";
            var entriesDir = $"{tableDir}/Entries";
            Directory.CreateDirectory(entriesDir);
            
            var tableConfigSource = $"../Database/{table.Name.ToBase64PathSafe()}/State.json";
            if (File.Exists(tableConfigSource))
                File.Copy(tableConfigSource, $"{tableDir}/State.json");
            
            currentTables.Add(tableName);
            var tableState = state.Tables.GetValueOrAdd(tableName, () => new());
            
            // list entries
            HashSet<string> currentEntries = [];
            foreach (var entry in table.ListAbstractEntries())
            {
                currentEntries.Add(entry.Id);
                var entryState = tableState.Entries.GetValueOrDefault(entry.Id);
                
                await using var h = await entry.Lock.WaitReadAsync();
                
                if (entryState != null && entryState.Timestamp == entry.Metadata.Timestamp)
                    continue;
                
                // entry changed
                if (entryState == null)
                {
                    entryState = new(id, entry.Metadata.Timestamp);
                    tableState.Entries[entry.Id] = entryState;
                }
                else
                {
                    entryState.Timestamp = entry.Metadata.Timestamp;
                    entryState.Origin = id;
                }
                File.Copy(entry.Path, $"{entriesDir}/{entry.Id.ToBase64PathSafe()}.json");
                
                // list files
                HashSet<string> currentFiles = [];
                bool filesDirCreated = false;
                var filesDir = $"{tableDir}/Files/{entry.Id.ToBase64PathSafe()}";
                foreach (var (fileName, fileData) in entry.Metadata.Files)
                {
                    currentFiles.Add(fileName);
                    var fileState = entryState.Files.GetValueOrDefault(fileName);
                    if (fileState != null && fileState.Timestamp == fileData.Timestamp)
                        continue;
                    
                    // file changed
                    if (!filesDirCreated)
                    {
                        Directory.CreateDirectory(filesDir);
                        filesDirCreated = true;
                    }
                    if (fileState == null)
                    {
                        fileState = new(id, fileData.Timestamp);
                        entryState.Files[fileName] = fileState;
                    }
                    else
                    {
                        fileState.Timestamp = fileData.Timestamp;
                        fileState.Origin = id;
                    }
                    File.Copy(entry.GetFilePath(fileName), $"{filesDir}/{fileName.ToBase64PathSafe()}");
                }
                
                // mark missing files as deleted
                foreach (var fileId in entryState.Files.Keys)
                    if (!currentFiles.Contains(fileId))
                        entryState.Files.Remove(fileId);
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
        
        await File.WriteAllBytesAsync($"{stateDir}/State.json", Serializers.DataContractJson.Serialize(state));
    }
    
    /// <summary>
    /// Restores all tables from the backup with the given ID.
    /// </summary>
    public static async Task RestoreAllAsync(string id)
    {
        var state = Serializers.DataContractJson.DeserializeNullable<BackupState>(await File.ReadAllBytesAsync($"{Server.Config.Backup.Directory}{id}/Database/State.json")) ?? throw new Exception("Failed to deserialize backup state");
        
        // list tables
        foreach (var (tableName, tableData) in state.Tables)
        {
            var tableDir = $"../Database/{tableName.ToBase64PathSafe()}";
            Directory.CreateDirectory(tableDir);
            
            var tableConfigSource = $"{Server.Config.Backup.Directory}{id}/Database/{tableName.ToBase64PathSafe()}/State.json";
            var tableConfigTarget = $"{tableDir}/State.json";
            if (File.Exists(tableConfigTarget))
                File.Delete(tableConfigTarget);
            if (File.Exists(tableConfigSource))
                File.Copy(tableConfigSource, tableConfigTarget);
            
            var table = Dictionary.GetValueOrDefault(tableName);
            if (table == null)
            {
                // restore table in the raw file system
                Directory.Delete(tableDir, true);
                Directory.CreateDirectory($"{tableDir}/Entries");
                Directory.CreateDirectory($"{tableDir}/Files");
                
                foreach (var (entryId, entryData) in tableData.Entries)
                {
                    File.Copy($"{Server.Config.Backup.Directory}{entryData.Origin}/Database/{tableName.ToBase64PathSafe()}/Entries/{entryId.ToBase64PathSafe()}.json", $"{tableDir}/Entries/{entryId.ToBase64PathSafe()}.json");
                    bool filesDirCreated = false;
                    var filesDir = $"{tableDir}/Files/{entryId.ToBase64PathSafe()}";
                    foreach (var (fileName, fileData) in entryData.Files)
                    {
                        if (!filesDirCreated)
                        {
                            Directory.CreateDirectory(filesDir);
                            filesDirCreated = true;
                        }
                        File.Copy($"{Server.Config.Backup.Directory}{fileData.Origin}/Database/{tableName.ToBase64PathSafe()}/Files/{entryId.ToBase64PathSafe()}/{fileName.ToBase64PathSafe()}", $"{filesDir}/{fileName.ToBase64PathSafe()}");
                    }
                }
            }
            else
            {
                // restore table efficiently
                foreach (var (entryId, entryData) in tableData.Entries)
                {
                    AsyncReaderWriterLockHolder locker;
                    if (table.TryGetAbstractEntry(entryId, out var entry))
                        locker = await entry.Lock.WaitWriteAsync();
                    else
                        (entry, locker) = await table.CreateAndLockBlankAbstractEntryAsync(entryId);
                    
                    await using (locker)
                    {
                        if (entry.Metadata.Timestamp == entryData.Timestamp)
                            continue;
                    
                        bool filesDirCreated = false;
                        var filesDir = $"{tableDir}/Files/{entryId.ToBase64PathSafe()}";
                        foreach (var (fileName, fileData) in entryData.Files)
                        {
                            if (entry.Metadata.Files.TryGetValue(fileName, out var fd) && fd.Timestamp == entryData.Timestamp)
                                continue;
                            
                            if (!filesDirCreated)
                            {
                                Directory.CreateDirectory(filesDir);
                                filesDirCreated = true;
                            }
                            if (File.Exists($"{filesDir}/{fileName.ToBase64PathSafe()}"))
                                File.Delete($"{filesDir}/{fileName.ToBase64PathSafe()}");
                            File.Copy($"{Server.Config.Backup.Directory}{fileData.Origin}/Database/{tableName.ToBase64PathSafe()}/Files/{entryId.ToBase64PathSafe()}/{fileName.ToBase64PathSafe()}", $"{filesDir}/{fileName.ToBase64PathSafe()}");
                        }
                        
                        foreach (var fileId in entry.Metadata.Files.Keys.ToList())
                            if (!entryData.Files.ContainsKey(fileId))
                                File.Delete($"{filesDir}/{fileId.ToBase64PathSafe()}");
                        
                        entry.SetBytes(await File.ReadAllBytesAsync($"{Server.Config.Backup.Directory}{entryData.Origin}/Database/{tableName.ToBase64PathSafe()}/Entries/{entryId.ToBase64PathSafe()}.json"));
                    }
                }
                
                // delete remaining entries
                foreach (var entry in table.ListAbstractEntries())
                    if (!tableData.Entries.ContainsKey(entry.Id))
                        await table.DeleteByIdAsync(entry.Id);
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
        if (Server.Config.Database.ClusterNodes.Count == 0)
            return;

        if (Server.GetCertificate(Server.Config.Database.CertificateDomain) == null)
        {
            Console.WriteLine($"No certificate found for database (domain \"{Server.Config.Database.CertificateDomain}\"), database will not connect to the cluster.");
            Server.Config.Database.ClusterNodes = [];
            return;
        }

        await Task.WhenAll(Server.Config.Database.ClusterNodes.Select(node => node.MarkSelfAsync()));
        
        if (_Self == null)
        {
            Console.WriteLine("Could not identify any node as this process, database will not connect to the cluster.");
            Server.Config.Database.ClusterNodes = [];
        }
    }
    
    /// <summary>
    /// Starts connection monitors for all cluster nodes.
    /// </summary>
    public static void StartMonitoringConnections()
    {
        foreach (var node in Server.Config.Database.ClusterNodes.Where(node => node.IsReachable))
            _ = node.MonitorConnection();
    }
    
    /// <summary>
    /// Stops the connection monitors for all cluster nodes.
    /// </summary>
    public static void StopMonitoringConnections()
    {
        foreach (var node in Server.Config.Database.ClusterNodes.Where(node => node.IsReachable))
            node.StopMonitoringConnection();
    }
}