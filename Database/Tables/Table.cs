using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Database;

public delegate void TransactionNullableDelegate<T>(ref T? value) where T : AbstractTableValue;

public delegate R TransactionNullableDelegate<T, out R>(ref T? value) where T : AbstractTableValue;

public delegate void TransactionNullableWithFilesDelegate<T>(ref T? value, ref List<IFileAction> fileActions) where T : AbstractTableValue;

public delegate R TransactionNullableWithFilesDelegate<T, out R>(ref T? value, ref List<IFileAction> fileActions) where T : AbstractTableValue;

public delegate void TransactionDelegate<T>(ref T value) where T : AbstractTableValue;

public delegate R TransactionDelegate<T, out R>(ref T value) where T : AbstractTableValue;

public delegate void TransactionWithFilesDelegate<T>(ref T value, ref List<IFileAction> fileActions) where T : AbstractTableValue;

public delegate R TransactionWithFilesDelegate<T, out R>(ref T value, ref List<IFileAction> fileActions) where T : AbstractTableValue;

/// <summary>
/// Contains the table functionality.
/// </summary>
public class Table<T> : AbstractTable where T : AbstractTableValue
{
    /// <summary>
    /// The dictionary of entries indexed by their IDs.
    /// </summary>
    private Dictionary<string, TableEntry<T>> Data = [];

    /// <summary>
    /// The lock to use when creating new entries.
    /// </summary>
    private ReaderWriterLockSlim CreationLock = new();
    
    /// <summary>
    /// Returns the list of indices to update.
    /// </summary>
    protected virtual IEnumerable<ITableIndex<T>> Indices => [];
    
    /// <summary>
    /// Returns the table with the given name, or loads/creates it if it isn't present already.
    /// </summary>
    public static Table<T> Import(string name)
        => Tables.Dictionary.TryGetValue(name, out AbstractTable? existingTable) ? (Table<T>)existingTable : new Table<T>(name);
    
    /// <summary>
    /// Creates a new table object with the given name and loads the entries.
    /// </summary>
    protected Table(string name)
        : base(name)
    {
        Tables.Dictionary[name] = this;
        Initialize();
    }

    /// <summary>
    /// Loads the table entries for the current table.
    /// </summary>
    private void Initialize()
    {
        string path = "../Database/" + Name.ToBase64PathSafe();
        string oldPath = "../Database/" + Name;
        string oldBuffer = "../Database/Buffer/" + Name;
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/Entries");
        Directory.CreateDirectory(path + "/Files");
        Directory.CreateDirectory(path + "/Trash/Entries");
        Directory.CreateDirectory(path + "/Trash/Files");
        Directory.CreateDirectory(path + "/Buffer/Entries");
        Directory.CreateDirectory(path + "/Buffer/Files");

        // delete entry buffer
        foreach (var keyFile in new DirectoryInfo(path + "/Buffer/Entries").GetFiles("*.json", SearchOption.TopDirectoryOnly))
            keyFile.Delete();

        // delete file buffer
        foreach (var filesDir in new DirectoryInfo(path + "/Buffer/Files").GetDirectories("*", SearchOption.TopDirectoryOnly))
            filesDir.Delete(true);

        // restore corrupted entries
        foreach (var keyFile in new DirectoryInfo(path + "/Trash/Entries").GetFiles("*.json", SearchOption.TopDirectoryOnly))
        {
            string fileName = keyFile.Name;
            string targetPath = $"{path}/Entries/{fileName}";
            if (File.Exists(targetPath))
                File.Delete(targetPath);
            keyFile.MoveTo(targetPath);
            
            Console.WriteLine($"Restored database entry: \"{Name} / {fileName[..^5].FromBase64PathSafe()}\"");
        }

        // restore corrupted files
        foreach (var filesDir in new DirectoryInfo(path + "/Trash/Files").GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            string encId = filesDir.Name;
            Directory.CreateDirectory($"{path}/Files/{encId}");
            foreach (var file in filesDir.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                string targetPath = $"{path}/Files/{encId}/{file.Name}";
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                file.MoveTo(targetPath);
                
                Console.WriteLine($"Restored database file: \"{Name} / {encId.FromBase64PathSafe()} / {file.Name.FromBase64PathSafe()}\"");
            }
            filesDir.Delete(true);
        }
        
        // load entries from old table structure
        if (Directory.Exists(oldPath))
        {
            foreach (var keyFile in new DirectoryInfo(oldPath).GetFiles("*.json", SearchOption.TopDirectoryOnly))
            {
                string id = keyFile.Name[..^5];
                byte[] serialized = File.ReadAllBytes(keyFile.FullName);
                var value = Serialization.Deserialize<T>(Name, id, serialized);
                if (value == null)
                {
                    Console.WriteLine($"Failed to deserialize old database entry \"{Name} / {id}\" for migration.");
                    continue;
                }
                serialized = Serialization.Serialize(value);
                string targetPath = $"{path}/Entries/{id.ToBase64PathSafe()}.json";
                File.WriteAllBytes(targetPath, serialized);
                keyFile.Delete();
            }
            
            if (new DirectoryInfo(oldPath).GetFiles("*", SearchOption.AllDirectories).Length == 0)
                Directory.Delete(oldPath, true);
        }
        
        if (Directory.Exists(oldBuffer))
            Directory.Delete(oldBuffer, true);
        if (Directory.Exists("../Database/Buffer") && new DirectoryInfo("../Database/Buffer").GetDirectories("*", SearchOption.TopDirectoryOnly).Length == 0)
            Directory.Delete("../Database/Buffer", true);

        // load entries
        foreach (var keyFile in new DirectoryInfo(path + "/Entries").GetFiles("*.json", SearchOption.TopDirectoryOnly))
        {
            string id = keyFile.Name[..^5].FromBase64PathSafe();
            byte[] serialized = File.ReadAllBytes(keyFile.FullName);
            var entry = new TableEntry<T>(Name, id, serialized);
            Data[id] = entry;
            
            UpdateIndices(entry);
        }
    }

    internal override Dictionary<string, MinimalTableValue> GetState()
        => Data.ToDictionary(x => x.Key, x => x.Value.EntryInfo);

    internal override void CheckAndFix()
    {
        // source nodes
        var nodes = GetReachableNodes();
        
        // check for corruption
        foreach (var entry in Data.Values)
        {
            if (entry.SerializedValue != null)
            {
                var memory = entry.SerializedValue.Data;
                var hash = entry.SerializedValue.Hash;
                var disk = File.ReadAllBytes(entry.Path);
            
                if (memory.ToMD5().SequenceEqual(hash))
                {
                    if (!entry.SerializedValue.Data.SequenceEqual(disk))
                    {
                        // re-write disk
                        File.WriteAllBytes(entry.Path, memory);
                        Console.WriteLine($"Fixed corrupt entry on disk: {entry.ReadableName}");
                    }
                }
                else if (disk.ToMD5().SequenceEqual(hash))
                {
                    // re-write memory
                    entry.SerializedValue.Data = disk;
                    Console.WriteLine($"Fixed corrupt entry in memory: {entry.ReadableName}");
                }
                else if (disk.SequenceEqual(memory))
                {
                    // re-write hash
                    entry.SerializedValue.Hash = memory.ToMD5();
                    Console.WriteLine($"Fixed corrupt entry hash: {entry.ReadableName}");
                }
                else
                {
                    // pull
                    entry.Lock.EnterWriteLock();
                    try
                    {
                        bool unresolved = true;
                        foreach (var node in nodes)
                        {
                            var remoteState = node.PullEntry(this, entry.Id);
                            if (remoteState != null)
                            {
                                entry.SetBytes(remoteState);
                                
                                if (!DownloadAllFiles(node, entry))
                                    Console.WriteLine($"Failed to re-download files for corrupt entry: {entry.ReadableName}");
                                
                                UpdateIndices(entry);
                                
                                Console.WriteLine($"Fixed corrupt entry using remote: {entry.ReadableName}");
                                unresolved = false;
                                break;
                            }
                        }
                        if (unresolved)
                            Console.WriteLine($"Failed to pull corrupt entry from remotes: {entry.ReadableName}");
                    }
                    finally
                    {
                        entry.Lock.ExitWriteLock();
                    }
                }
            }
        }
        
        // collect possibly deletable entries
        var idsToDelete = Data.Values.Where(entry => entry.EntryInfo.Deleted).Select(entry => entry.Id).ToList();
        
        // sync with other nodes
        foreach (var node in nodes)
        {
            var state = node.PullState(this);
            if (state != null)
            {
                SyncFrom(node, state);
                idsToDelete = idsToDelete.Where(id => !state.TryGetValue(id, out var info) || info.Deleted).ToList();
            }
            else idsToDelete = [];
        }
        
        // delete deletable entries
        foreach (var id in idsToDelete)
        {
            if (!Data.TryGetValue(id, out var entry))
                continue;
            entry.Lock.EnterWriteLock();
            try
            {
                entry.DeleteFileDirectories();
                entry.DeleteEntryFiles();
                Data.Remove(id);
            }
            finally
            {
                entry.Lock.ExitWriteLock();
            }
        }
        
        // delete obsolete file directories and files
        foreach (var directoryPrefix in (IEnumerable<string>)["", "Buffer/", "Trash/"])
            foreach (var filesDir in new DirectoryInfo($"../Database/{Name.ToBase64PathSafe()}/{directoryPrefix}Files").GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var id = filesDir.Name.FromBase64PathSafe();
                if (Data.TryGetValue(id, out var entry) && entry.EntryInfo.Files.Count > 0)
                {
                    entry.Lock.EnterWriteLock();
                    try
                    {
                        foreach (var file in filesDir.GetFiles("*", SearchOption.TopDirectoryOnly))
                        {
                            var fileId = file.Name.FromBase64PathSafe();
                            if (!entry.EntryInfo.Files.ContainsKey(fileId))
                                file.Delete();
                        }
                        continue;
                    }
                    finally
                    {
                        entry.Lock.ExitWriteLock();
                    }
                }
                
                filesDir.Delete(true);
            }
    }
    
    internal override void SyncFrom(ClusterNode node, Dictionary<string, MinimalTableValue> state)
    {
        foreach (var (id, stateInfo) in state)
        {
            // create or update entry
            if (!Data.TryGetValue(id, out var entry) || stateInfo.Timestamp > entry.EntryInfo.Timestamp)
            {
                var serialized = node.PullEntry(this, id);
                if (serialized == null)
                    continue;
                UpdateEntry(node, id, serialized);
            }
            
            // download missing files
            if (Data.TryGetValue(id, out entry) && entry.EntryInfo.Files.Keys.Any(fileId => !File.Exists(entry.GetFilePath(fileId))))
            {
                entry.Lock.EnterWriteLock();
                try
                {
                    Directory.CreateDirectory(entry.FileBasePath);
                    Directory.CreateDirectory(entry.BufferFileBasePath);
                    foreach (var fileId in entry.EntryInfo.Files.Keys.Where(fileId => !File.Exists(entry.GetFilePath(fileId))))
                        if (node.PullFile(this, id, fileId, entry.GetBufferFilePath(fileId)))
                            File.Move(entry.GetBufferFilePath(fileId), entry.GetFilePath(fileId));
                }
                finally
                {
                    entry.Lock.ExitWriteLock();
                }
            }
        }
    }
    
    internal override void UpdateEntry(ClusterNode node, string id, byte[] serialized)
    {
        var remoteInfo = Serialization.Deserialize<MinimalTableValue>(Name, id, serialized);
        if (remoteInfo == null)
        {
            Console.WriteLine($"Failed to deserialize pulled entry: \"{Name} / {id}\"");
            return;
        }
        
        // re-check
        if (Data.TryGetValue(id, out var entry))
        {
            // out of date
            entry.Lock.EnterWriteLock();
            if (remoteInfo.Timestamp <= entry.EntryInfo.Timestamp)
            {
                entry.Lock.ExitWriteLock();
                return;
            }
        }
        else
        {
            // create and lock entry
            entry = CreateAndLockBlankEntry(id);
        }
        
        // apply changes to entry and files
        try
        {
            entry.CreateFileDirectories();

            // move deleted and outdated files to trash
            foreach (var (fileId, fileInfo) in entry.EntryInfo.Files)
                if (!remoteInfo.Files.TryGetValue(fileId, out var remoteFile) || remoteFile.Timestamp != fileInfo.Timestamp)
                {
                    var filePath = entry.GetFilePath(fileId);
                    if (File.Exists(filePath))
                        File.Move(filePath, entry.GetTrashFilePath(fileId));
                }

            // download new and updated files
            foreach (var fileId in remoteInfo.Files.Keys)
            {
                var filePath = entry.GetFilePath(fileId);
                if (!File.Exists(filePath))
                {
                    var bufferFilePath = entry.GetBufferFilePath(fileId);
                    if (node.PullFile(this, entry.Id, fileId, bufferFilePath))
                        File.Move(bufferFilePath, filePath);
                    var trashFilePath = entry.GetTrashFilePath(fileId);
                    if (File.Exists(trashFilePath))
                        File.Delete(trashFilePath);
                }
            }
    
            // update entry
            var oldValue = entry.Deserialize();
            entry.SetBytes(serialized, remoteInfo);
            var newValue = entry.Deserialize();
            UpdateIndices(id, newValue);
        
            // delete trash (only contains removed files)
            if (Directory.Exists(entry.TrashFileBasePath))
                Directory.Delete(entry.TrashFileBasePath, true);

            // clean up directories if no files remain
            if (remoteInfo.Files.Count == 0)
            {
                if (Directory.Exists(entry.FileBasePath))
                    Directory.Delete(entry.FileBasePath, true);
                if (Directory.Exists(entry.BufferFileBasePath))
                    Directory.Delete(entry.BufferFileBasePath, true);
            }
            
            entry.CallChangedEvent(oldValue, newValue);
        }
        finally
        {
            entry.Lock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Downloads all attached files for the given table entry from the given node.
    /// </summary>
    private bool DownloadAllFiles(ClusterNode node, TableEntry<T> entry)
    {
        if (entry.EntryInfo.Files.Count == 0)
            return true;
        
        bool success = true;
        
        entry.DeleteFileDirectories();
        entry.CreateFileDirectories();
        
        foreach (var fileId in entry.EntryInfo.Files.Keys)
        {
            var filePath = entry.GetFilePath(fileId);
            var bufferFilePath = entry.GetBufferFilePath(fileId);
            if (node.PullFile(this, entry.Id, fileId, bufferFilePath))
                File.Move(bufferFilePath, filePath);
            else success = false;
        }
        
        return success;
    }
    
    /// <summary>
    /// Updates all table indices using the given entry ID and its value.
    /// </summary>
    private void UpdateIndices(string id, T? value)
    {
        foreach (var index in Indices)
            index.Update(id, value);
    }
    
    /// <summary>
    /// Updates all table indices using the given entry.
    /// </summary>
    private void UpdateIndices(TableEntry<T> entry)
        => UpdateIndices(entry.Id, entry.Deserialize());
    
    internal override AbstractTableEntry CreateAndLockBlankAbstractEntry(string id)
        => CreateAndLockBlankEntry(id);
    
    /// <summary>
    /// Creates a new locked table entry with the given ID.
    /// </summary>
    private TableEntry<T> CreateAndLockBlankEntry(string id)
    {
        CreationLock.EnterWriteLock();
        try
        {
            var entry = new TableEntry<T>(Name, id, Serialization.Serialize(new MinimalTableValue { Deleted = true, Timestamp = 0 }));
            entry.Lock.EnterWriteLock();
            Data[id] = entry;
            return entry;
        }
        finally
        {
            CreationLock.ExitWriteLock();
        }
    }
    
    internal override ClusterNode[] GetReachableNodes()
    {
        var nodes = Server.Config.Database.Cluster.Where(node => node.IsReachable && (node.TableNames == null || node.TableNames.Contains(Name))).ToArray();
        Random.Shared.Shuffle(nodes);
        return nodes;
    }
    
    public override Version GetTypeVersion()
        => Tables.GetTypeVersion(typeof(T));
    
    public override Version GetMinVersion()
        => new(0, 0, 0, 0);
    
    /// <summary>
    /// Returns whether the table contains an entry with the given ID.
    /// </summary>
    public bool ContainsId(string id)
        => Data.ContainsKey(id);
    
    /// <summary>
    /// Finds the value of the table entry with the given ID.
    /// </summary>
    public bool TryGetValue(string id, [MaybeNullWhen(false)] out T value)
        => (value = GetByIdNullable(id)) != null;

    /// <summary>
    /// Finds the entry with the given ID.
    /// </summary>
    public bool TryGetEntry(string id, [MaybeNullWhen(false)] out TableEntry<T> entry)
        => Data.TryGetValue(id, out entry);

    internal override bool TryGetAbstractEntry(string id, [MaybeNullWhen(false)] out AbstractTableEntry entry)
    {
        if (TryGetEntry(id, out var entry2))
        {
            entry = entry2;
            return true;
        }
        else
        {
            entry = null;
            return false;
        }
    }
    
    /// <summary>
    /// Enumerates all non-deleted values by the given entry IDs.
    /// </summary>
    public IEnumerable<T> EnumerateExistingByIds(IEnumerable<string> ids)
        => ids.SelectWhereNotNull(GetByIdNullable);
    
    /// <summary>
    /// Returns the value of the entry with the given ID, or throws an exception if no such entry exists.
    /// </summary>
    public T GetById(string? id)
        => GetByIdNullable(id) ?? throw new NullReferenceException();
    
    /// <summary>
    /// Returns the value of the entry with the given ID, or returns null if no such entry exists.
    /// </summary>
    public T? GetByIdNullable(string? id)
    {
        if (id == null)
            return null;
        
        if (Data.TryGetValue(id, out var entry))
        {
            entry.Lock.EnterReadLock();
            try
            {
                return entry.Deserialize();
            }
            finally
            {
                entry.Lock.ExitReadLock();
            }
        }
        else return null;
    }
    
    /// <summary>
    /// Lists all non-deleted values in the table.
    /// </summary>
    public List<T> ListAll()
    {
        List<T> result = [];
        foreach (var entry in Data.Values)
        {
            entry.Lock.EnterReadLock();
            if (!entry.EntryInfo.Deleted)
            {
                var value = entry.Deserialize();
                if (value != null)
                    result.Add(value);
            }
            entry.Lock.ExitReadLock();
        }
        return result;
    }
    
    internal override List<AbstractTableEntry> ListAbstractEntries()
        => [..Data.Values];
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public T Transaction(string id, TransactionDelegate<T> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> _) =>
        {
            if (value == null)
                throw new NullReferenceException();
            
            action(ref value);
            return value;
        }) ?? throw new NullReferenceException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public T Transaction(string id, TransactionWithFilesDelegate<T> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> fileActions) =>
        {
            if (value == null)
                throw new NullReferenceException();
            
            action(ref value, ref fileActions);
            return value;
        }) ?? throw new NullReferenceException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action.<br/>
    /// If no entry with the given ID was found, nothing happens.
    /// </summary>
    public void TransactionIgnoreNull(string id, TransactionDelegate<T> action)
        => TransactionNullable(id, (ref T? value, ref List<IFileAction> _) =>
        {
            if (value != null)
                action(ref value);
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action.<br/>
    /// If no entry with the given ID was found, nothing happens.
    /// </summary>
    public void TransactionIgnoreNull(string id, TransactionWithFilesDelegate<T> action)
        => TransactionNullable(id, (ref T? value, ref List<IFileAction> fileActions) =>
        {
            if (value != null)
                action(ref value, ref fileActions);
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public R TransactionAndGet<R>(string id, TransactionDelegate<T, R> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> _) =>
        {
            if (value == null)
                throw new NullReferenceException();
            
            return action(ref value);
        }) ?? throw new NullReferenceException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public R TransactionAndGet<R>(string id, TransactionWithFilesDelegate<T, R> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> fileActions) =>
        {
            if (value == null)
                throw new NullReferenceException();
            
            return action(ref value, ref fileActions);
        }) ?? throw new NullReferenceException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, null is returned.
    /// </summary>
    public T? TransactionNullable(string id, TransactionNullableDelegate<T> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> _) =>
        {
            action(ref value);
            return value;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, null is returned.
    /// </summary>
    public T? TransactionNullable(string id, TransactionNullableWithFilesDelegate<T> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> fileActions) =>
        {
            action(ref value, ref fileActions);
            return value;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found and a value was set in the action, an entry will be created. 
    /// </summary>
    public R TransactionNullableAndGet<R>(string id, TransactionNullableDelegate<T, R> action)
        => TransactionNullableAndGet(id, (ref T? value, ref List<IFileAction> _) => action(ref value));
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found and a value was set in the action, an entry will be created. 
    /// </summary>
    public R TransactionNullableAndGet<R>(string id, TransactionNullableWithFilesDelegate<T, R> action)
    {
        var nodes = GetReachableNodes();
        
        var entry = Data.GetValueOrDefault(id);
        
        var request = entry == null ? null : LockRequest.CreateLocal(entry);
        request?.WaitUntilReady();
        
        entry?.Lock.EnterWriteLock();
        try
        {
            var value = entry?.Deserialize();
            List<IFileAction> fileActions = [];

            R result;
            try
            {
                result = action(ref value, ref fileActions);
            }
            catch
            {
                if (entry != null && request != null)
                {
                    foreach (var node in nodes)
                        _ = node.SendCancelAsync(this, id, request.Timestamp, request.Randomness);
                    
                    LockRequest.Delete(entry, request.Timestamp, request.Randomness);
                }
                throw;
            }
        
            var timestamp = DateTime.UtcNow.Ticks;
            byte[] serialized;
            if (value != null)
            {
                foreach (var fileAction in fileActions)
                    fileAction.Prepare(value);
                foreach (var fileAction in fileActions)
                    fileAction.Commit(value, timestamp);
                
                value.AssemblyVersion = GetTypeVersion();
                value.Timestamp = timestamp;
                serialized = Serialization.Serialize(value);
            }
            else
            {
                entry?.DeleteFileDirectories();
                
                serialized = Serialization.Serialize(new MinimalTableValue
                {
                    Deleted = true,
                    AssemblyVersion = GetTypeVersion(),
                    Timestamp = timestamp
                });
            }
        
            entry ??= CreateAndLockBlankEntry(id);
            
            if (value != null)
                value.ContainingEntry = entry;
        
            entry.SetBytes(serialized);
            
            UpdateIndices(id, value);
            
            foreach (var node in nodes)
                _ = node.PushChangeAsync(this, id, request?.Timestamp ?? 0, request?.Randomness ?? "none", serialized);
            
            if (request != null)
                LockRequest.Delete(entry, request.Timestamp, request.Randomness);
            
            return result;
        }
        finally
        {
            entry?.Lock.ExitWriteLock();
        }
    }
    
    public override bool Delete(string id)
        => Data.ContainsKey(id) && TransactionNullableAndGet(id, (ref T? value) =>
        {
            bool exists = value != null;
            value = null;
            return exists;
        });
    
    /// <summary>
    /// Generates a random non-existing entry ID with the given length.
    /// </summary>
    public string GenerateId(int length)
    {
        string id;
        do id = Parsers.RandomString(length);
        while (Data.ContainsKey(id));
        return id;
    }
    
    /// <summary>
    /// Creates a new entry with a random non-existing ID (with the given length) and stores the given value in it.
    /// </summary>
    public T Create(int idLength, T value)
    {
        var id = GenerateId(idLength);
        TransactionNullable(id, (ref T? v) => v = value);
        return value;
    }
}