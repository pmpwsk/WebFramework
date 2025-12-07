using System.Diagnostics.CodeAnalysis;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

public delegate void TransactionNullableDelegate<T>(TransactionData<T?> transaction) where T : AbstractTableValue;

public delegate R TransactionNullableDelegate<T, out R>(TransactionData<T?> transaction) where T : AbstractTableValue;

public delegate void TransactionDelegate<T>(TransactionData<T> transaction) where T : AbstractTableValue;

public delegate R TransactionDelegate<T, out R>(TransactionData<T> transaction) where T : AbstractTableValue;

public delegate Task AsyncTransactionNullableDelegate<T>(TransactionData<T?> transaction) where T : AbstractTableValue;

public delegate Task<R> AsyncTransactionNullableDelegate<T, R>(TransactionData<T?> transaction) where T : AbstractTableValue;

public delegate Task AsyncTransactionDelegate<T>(TransactionData<T> transaction) where T : AbstractTableValue;

public delegate Task<R> AsyncTransactionDelegate<T, R>(TransactionData<T> transaction) where T : AbstractTableValue;

/// <summary>
/// Contains the table functionality.
/// </summary>
public class Table<T> : AbstractTable, IDisposable where T : AbstractTableValue
{
    /// <summary>
    /// The dictionary of entries indexed by their IDs.
    /// </summary>
    private Dictionary<string, TableEntry<T>> Data = [];

    /// <summary>
    /// The lock to use when creating new entries.
    /// </summary>
    private AsyncLock CreationLock = new();
    
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
        Initialize().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Loads the table entries for the current table.
    /// </summary>
    private async Task Initialize()
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
                byte[] serialized = await File.ReadAllBytesAsync(keyFile.FullName);
                var value = Serialization.Deserialize<T>(this, id, serialized);
                if (value == null)
                {
                    Console.WriteLine($"Failed to deserialize old database entry \"{Name} / {id}\" for migration.");
                    continue;
                }
                serialized = Serialization.Serialize(value);
                string targetPath = $"{path}/Entries/{id.ToBase64PathSafe()}.json";
                await File.WriteAllBytesAsync(targetPath, serialized);
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
            byte[] serialized = await File.ReadAllBytesAsync(keyFile.FullName);
            var entry = new TableEntry<T>(this, id, serialized);
            Data[id] = entry;
            
            await UpdateIndicesAsync(entry);
        }
    }

    internal override Dictionary<string, MinimalTableValue> GetState()
        => Data.ToDictionary(x => x.Key, x => x.Value.EntryInfo);

    internal override async Task CheckAndFixAsync()
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
                var disk = await File.ReadAllBytesAsync(entry.Path);
            
                if (memory.ToMD5().SequenceEqual(hash))
                {
                    if (!entry.SerializedValue.Data.SequenceEqual(disk))
                    {
                        // re-write disk
                        await File.WriteAllBytesAsync(entry.Path, memory);
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
                else //pull
                {
                    await using var h = await entry.Lock.WaitWriteAsync();
                        
                    bool unresolved = true;
                    foreach (var node in nodes)
                    {
                        var remoteState = await node.PullEntryAsync(this, entry.Id);
                        if (remoteState != null)
                        {
                            entry.SetBytes(remoteState);
                            
                            if (!await DownloadAllFilesAsync(node, entry))
                                Console.WriteLine($"Failed to re-download files for corrupt entry: {entry.ReadableName}");
                            
                            await UpdateIndicesAsync(entry);
                            
                            Console.WriteLine($"Fixed corrupt entry using remote: {entry.ReadableName}");
                            unresolved = false;
                            break;
                        }
                    }
                    if (unresolved)
                        Console.WriteLine($"Failed to pull corrupt entry from remotes: {entry.ReadableName}");
                }
            }
        }
        
        // collect possibly deletable entries
        var idsToDelete = Data.Values.Where(entry => entry.EntryInfo.Deleted).Select(entry => entry.Id).ToList();
        
        // sync with other nodes
        foreach (var node in nodes)
        {
            var state = await node.PullStateAsync(this);
            if (state != null)
            {
                await SyncFromAsync(node, state);
                idsToDelete = idsToDelete.Where(id => !state.TryGetValue(id, out var info) || info.Deleted).ToList();
            }
            else idsToDelete = [];
        }
        
        // delete deletable entries
        foreach (var id in idsToDelete)
        {
            if (!Data.TryGetValue(id, out var entry))
                continue;
            
            await using var h = await entry.Lock.WaitWriteAsync();
            
            entry.DeleteFileDirectories();
            entry.DeleteEntryFiles();
            Data.Remove(id);
        }
        
        // delete obsolete file directories and files
        foreach (var directoryPrefix in (IEnumerable<string>)["", "Buffer/", "Trash/"])
            foreach (var filesDir in new DirectoryInfo($"../Database/{Name.ToBase64PathSafe()}/{directoryPrefix}Files").GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var id = filesDir.Name.FromBase64PathSafe();
                if (Data.TryGetValue(id, out var entry) && entry.EntryInfo.Files.Count > 0)
                {
                    await using var h = await entry.Lock.WaitWriteAsync();
                    
                    foreach (var file in filesDir.GetFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        var fileId = file.Name.FromBase64PathSafe();
                        if (!entry.EntryInfo.Files.ContainsKey(fileId))
                            file.Delete();
                    }
                    continue;
                }
                
                filesDir.Delete(true);
            }
    }
    
    internal override async Task SyncFromAsync(ClusterNode node, Dictionary<string, MinimalTableValue> state)
    {
        foreach (var (id, stateInfo) in state)
        {
            // create or update entry
            if (!Data.TryGetValue(id, out var entry) || stateInfo.Timestamp > entry.EntryInfo.Timestamp)
            {
                var serialized = await node.PullEntryAsync(this, id);
                if (serialized == null)
                    continue;
                await UpdateEntryAsync(node, id, serialized);
            }
            
            // download missing files
            if (Data.TryGetValue(id, out entry) && entry.EntryInfo.Files.Keys.Any(fileId => !File.Exists(entry.GetFilePath(fileId))))
            {
                await using var h = await entry.Lock.WaitWriteAsync();
                
                Directory.CreateDirectory(entry.FileBasePath);
                Directory.CreateDirectory(entry.BufferFileBasePath);
                foreach (var fileId in entry.EntryInfo.Files.Keys.Where(fileId => !File.Exists(entry.GetFilePath(fileId))))
                    if (await node.PullFileAsync(this, id, fileId, entry.GetBufferFilePath(fileId)))
                        File.Move(entry.GetBufferFilePath(fileId), entry.GetFilePath(fileId));
            }
        }
    }
    
    internal override async Task UpdateEntryAsync(ClusterNode node, string id, byte[] serialized)
    {
        var remoteInfo = Serialization.Deserialize<MinimalTableValue>(this, id, serialized);
        if (remoteInfo == null)
        {
            Console.WriteLine($"Failed to deserialize pulled entry: \"{Name} / {id}\"");
            return;
        }
        
        // re-check
        AsyncReaderWriterLockHolder locker;
        if (Data.TryGetValue(id, out var entry))
        {
            // out of date
            locker = await entry.Lock.WaitWriteAsync();
            if (remoteInfo.Timestamp <= entry.EntryInfo.Timestamp)
            {
                await locker.DisposeAsync();
                return;
            }
        }
        else
        {
            // create and lock entry
            (entry, locker) = await CreateAndLockBlankEntryAsync(id);
        }
        
        // apply changes to entry and files
        await using (locker)
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
                    if (await node.PullFileAsync(this, entry.Id, fileId, bufferFilePath))
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
            await UpdateIndicesAsync(id, newValue);
        
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
            
            await entry.CallChangedEventAsync(oldValue, newValue);
        }
    }
    
    /// <summary>
    /// Downloads all attached files for the given table entry from the given node.
    /// </summary>
    private async Task<bool> DownloadAllFilesAsync(ClusterNode node, TableEntry<T> entry)
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
            if (await node.PullFileAsync(this, entry.Id, fileId, bufferFilePath))
                File.Move(bufferFilePath, filePath);
            else success = false;
        }
        
        return success;
    }
    
    /// <summary>
    /// Updates all table indices using the given entry ID and its value.
    /// </summary>
    private async Task UpdateIndicesAsync(string id, T? value)
    {
        foreach (var index in Indices)
            await index.UpdateAsync(id, value);
    }
    
    /// <summary>
    /// Updates all table indices using the given entry.
    /// </summary>
    private Task UpdateIndicesAsync(TableEntry<T> entry)
        => UpdateIndicesAsync(entry.Id, entry.Deserialize());
    
    internal override async Task<(AbstractTableEntry Entry, AsyncReaderWriterLockHolder Locker)> CreateAndLockBlankAbstractEntryAsync(string id)
        => await CreateAndLockBlankEntryAsync(id);
    
    /// <summary>
    /// Creates a new locked table entry with the given ID.
    /// </summary>
    private async Task<(TableEntry<T> Entry, AsyncReaderWriterLockHolder Locker)> CreateAndLockBlankEntryAsync(string id)
    {
        using var h = await CreationLock.WaitAsync();
        
        var entry = new TableEntry<T>(this, id, Serialization.Serialize(new MinimalTableValue { Deleted = true, Timestamp = 0 }));
        var holder = await entry.Lock.WaitWriteAsync();
        Data[id] = entry;
        return (entry, holder);
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
    public async Task<List<T>> ListExistingByIdsAsync(IEnumerable<string> ids)
    {
        List<T> result = [];
        foreach (var id in ids)
        {
            var value = await GetByIdNullableAsync(id);
            if (value != null)
                result.Add(value);
        }
        return result;
    }
    
    /// <summary>
    /// Returns the value of the entry with the given ID, or throws an exception if no such entry exists.
    /// </summary>
    public async Task<T> GetByIdAsync(string? id)
        => await GetByIdNullableAsync(id) ?? throw new DatabaseEntryMissingException();
    
    /// <summary>
    /// Returns the value of the entry with the given ID, or returns null if no such entry exists.
    /// </summary>
    public async Task<T?> GetByIdNullableAsync(string? id)
    {
        if (id == null)
            return null;
        
        if (Data.TryGetValue(id, out var entry))
        {
            await using var h =await entry.Lock.WaitReadAsync();
            
            return entry.Deserialize();
        }
        else return null;
    }
    
    /// <summary>
    /// Lists all non-deleted values in the table.
    /// </summary>
    public async Task<List<T>> ListAllAsync()
    {
        List<T> result = [];
        foreach (var entry in Data.Values)
        {
            await using var h = await entry.Lock.WaitReadAsync();
            
            if (!entry.EntryInfo.Deleted)
            {
                var value = entry.Deserialize();
                if (value != null)
                    result.Add(value);
            }
        }
        return result;
    }
    
    internal override List<AbstractTableEntry> ListAbstractEntries()
        => [..Data.Values];
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public Task<T> TransactionAsync(string id, TransactionDelegate<T> action)
        => AsyncTransactionAsync(id, transaction =>
        {
            action(transaction);
            return Task.CompletedTask;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action.<br/>
    /// If no entry with the given ID was found, nothing happens.
    /// </summary>
    public Task TransactionIgnoreNullAsync(string id, TransactionDelegate<T> action)
        => AsyncTransactionIgnoreNullAsync(id, transaction =>
        {
            action(transaction);
            return Task.CompletedTask;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public Task<R> TransactionAndGetAsync<R>(string id, TransactionDelegate<T, R> action)
        => AsyncTransactionAndGetAsync(id, transaction => Task.FromResult(action(transaction)));
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, null is returned.
    /// </summary>
    public Task<T?> TransactionNullableAsync(string id, TransactionNullableDelegate<T> action)
        => AsyncTransactionNullableAsync(id, transaction =>
        {
            action(transaction);
            return Task.CompletedTask;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found and a value was set in the action, an entry will be created. 
    /// </summary>
    public Task<R> TransactionNullableAndGetAsync<R>(string id, TransactionNullableDelegate<T, R> action)
        => AsyncTransactionNullableAndGetAsync(id, transaction => Task.FromResult(action(transaction)));
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action, and returns the new value.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public Task<T> AsyncTransactionAsync(string id, AsyncTransactionDelegate<T> action)
        => AsyncTransactionNullableAndGetAsync(id, async transaction =>
        {
            if (transaction.Value == null)
                throw new DatabaseEntryMissingException();
            
            await action(new(transaction.Value, transaction.FileActions));
            return transaction.Value;
        }) ?? throw new DatabaseEntryMissingException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action.<br/>
    /// If no entry with the given ID was found, nothing happens.
    /// </summary>
    public Task AsyncTransactionIgnoreNullAsync(string id, AsyncTransactionDelegate<T> action)
        => AsyncTransactionNullableAsync(id, async transaction =>
        {
            if (transaction.Value != null)
                await action(new(transaction.Value, transaction.FileActions));
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found, an exception is thrown.
    /// </summary>
    public Task<R> AsyncTransactionAndGetAsync<R>(string id, AsyncTransactionDelegate<T, R> action)
        => AsyncTransactionNullableAndGetAsync(id, transaction =>
        {
            if (transaction.Value == null)
                throw new DatabaseEntryMissingException();
            
            return action(new(transaction.Value, transaction.FileActions));
        }) ?? throw new DatabaseEntryMissingException();
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found and a value was set in the action, an entry will be created. 
    /// </summary>
    public Task<T?> AsyncTransactionNullableAsync(string id, AsyncTransactionNullableDelegate<T> action)
        => AsyncTransactionNullableAndGetAsync(id, async transaction =>
        {
            await action(transaction);
            return transaction.Value;
        });
    
    /// <summary>
    /// Performs a transaction on the entry with the given ID by executing the given action and returns the action's result.<br/>
    /// If no entry with the given ID was found and a value was set in the action, an entry will be created. 
    /// </summary>
    public async Task<R> AsyncTransactionNullableAndGetAsync<R>(string id, AsyncTransactionNullableDelegate<T, R> action)
    {
        var nodes = GetReachableNodes();
        
        var entry = Data.GetValueOrDefault(id);
        
        R result;
        T? oldValue;
        T? newValue;
        
        var request = entry == null ? null : await LockRequest.CreateLocalAsync(entry);
        if (request != null)
            await request.WaitUntilReady();
        
        var locker = entry == null ? null : await entry.Lock.WaitWriteAsync();
        try
        {
            oldValue = entry?.Deserialize();
            
            TransactionData<T?> transaction = new(entry?.Deserialize());
            
            try
            {
                result = await action(transaction);
            }
            catch
            {
                if (entry != null && request != null)
                {
                    foreach (var node in nodes)
                        _ = node.SendCancelAsync(this, id, request.Timestamp, request.Randomness);
                    
                    await LockRequest.DeleteAsync(entry, request.Timestamp, request.Randomness);
                }
                throw;
            }
        
            var timestamp = DateTime.UtcNow.Ticks;
            byte[] serialized;
            if (transaction.Value != null)
            {
                foreach (var fileAction in transaction.FileActions)
                    fileAction.Prepare(transaction.Value);
                foreach (var fileAction in transaction.FileActions)
                    fileAction.Commit(transaction.Value, timestamp);
                
                transaction.Value.AssemblyVersion = GetTypeVersion();
                transaction.Value.Timestamp = timestamp;
                serialized = Serialization.Serialize(transaction.Value);
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
        
            if (entry == null)
                (entry, locker) = await CreateAndLockBlankEntryAsync(id);
            
            if (transaction.Value != null)
                transaction.Value.ContainingEntry = entry;
        
            entry.SetBytes(serialized);
            
            await UpdateIndicesAsync(id, transaction.Value);
            
            foreach (var node in nodes)
                _ = node.PushChangeAsync(this, id, request?.Timestamp ?? 0, request?.Randomness ?? "none", serialized);
            
            if (request != null)
                await LockRequest.DeleteAsync(entry, request.Timestamp, request.Randomness);
            
            newValue = transaction.Value;
        }
        finally
        {
            if (locker != null)
                await locker.DisposeAsync();
        }
        
        await entry.CallChangedEventAsync(oldValue, newValue);
            
        return result;
    }
    
    public override async Task<bool> DeleteAsync(string id)
        => Data.ContainsKey(id) && await TransactionNullableAndGetAsync(id, transaction =>
        {
            bool exists = transaction.Value != null;
            transaction.Value = null;
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
    public async Task<T> CreateAsync(int idLength, T value)
    {
        var id = GenerateId(idLength);
        await TransactionNullableAsync(id, transaction => transaction.Value = value);
        return value;
    }
    
    /// <summary>
    /// Modifies the given reference value with the current value and holds the transaction open until the returned transaction is disposed.
    /// </summary>
    public ModifyTransactionData StartModifying(ref T value)
    {
        ModifyTransactionData modify = new();
        ReadyWaiter waiter = new();
        T? newValue = null;
        _ = AsyncTransactionNullableAsync(value.Id, async transaction =>
        {
            transaction.FileActions = modify.FileActions;
            newValue = transaction.Value ?? throw new DatabaseEntryMissingException();
            // ReSharper disable once AccessToDisposedClosure
            await waiter.ReadyAsync();
            await modify.WaitAsync();
        });
        waiter.WaitAsync().GetAwaiter().GetResult();
        waiter.Dispose();
        value = newValue ?? throw new DatabaseEntryMissingException();
        return modify;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CreationLock.Dispose();
        foreach (var index in Indices)
            index.Dispose();
    }
}