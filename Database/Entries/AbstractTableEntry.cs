using System.Runtime.Serialization;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Contains the table entry functionality that doesn't require knowledge of the stored type.
/// </summary>
public abstract class AbstractTableEntry
{
    /// <summary>
    /// The name of the table the entry belongs to.
    /// </summary>
    public readonly string TableName;

    /// <summary>
    /// The ID to reference the entry.
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// A cached version of the serialized entry value, or null if caching is disabled.
    /// </summary>
    public RawFileContent? SerializedValue { get; protected set; }

    private MinimalTableValue _EntryInfo;
    /// <summary>
    /// The common part of the value's data.
    /// </summary>
    public MinimalTableValue EntryInfo
    {
        get => _EntryInfo;
        protected set
        {
            _EntryInfo = value;
            _EntryInfo.ContainingEntry = this;
        }
    }

    /// <summary>
    /// The lock used to lock the entry for reading and writing.
    /// </summary>
    internal ReaderWriterLockSlim Lock = new();
    
    /// <summary>
    /// The ongoing and queued lock requests, sorted by timestamp.
    /// </summary>
    internal SortedSet<LockRequest> LockRequests = [];
    
    /// <summary>
    /// The lock requests that were recently deleted.<br/>
    /// These are ignored when receiving them from other servers (who haven't deleted the request yet).
    /// </summary>
    internal ExpiringSet<string> DeletedLockRequests = new(Server.Config.Database.LockExpiration * 10, false);
    
    /// <summary>
    /// Creates a new entry for the given table, ID and serialized value.
    /// </summary>
    protected AbstractTableEntry(string tableName, string id, byte[] serialized)
    {
        TableName = tableName;
        Id = id;
        SerializedValue = Server.Config.Database.CacheEntries ? new(serialized) : null;
        _EntryInfo = Serialization.Deserialize<MinimalTableValue>(tableName, id, serialized) ?? throw new SerializationException();
        _EntryInfo.ContainingEntry = this;
    }
    
    /// <summary>
    /// The table name and ID, formatted to be easily readable in logs. 
    /// </summary>
    public string ReadableName => $"\"{TableName} / {Id}\"";
    
    /// <summary>
    /// The path to the serialized value.
    /// </summary>
    public string Path => $"../Database/{TableName.ToBase64PathSafe()}/Entries/{Id.ToBase64PathSafe()}.json";
    
    /// <summary>
    /// The path to the directory that contains attached files.
    /// </summary>
    public string FileBasePath => $"../Database/{TableName.ToBase64PathSafe()}/Files/{Id.ToBase64PathSafe()}";
    
    /// <summary>
    /// The path to the trashed serialized value.
    /// </summary>
    public string TrashPath => $"../Database/{TableName.ToBase64PathSafe()}/Trash/Entries/{Id.ToBase64PathSafe()}.json";
    
    /// <summary>
    /// The path to the directory that contains trashed attached files.
    /// </summary>
    public string TrashFileBasePath => $"../Database/{TableName.ToBase64PathSafe()}/Trash/Files/{Id.ToBase64PathSafe()}";
    
    /// <summary>
    /// The path to the buffered serialized value.
    /// </summary>
    public string BufferPath => $"../Database/{TableName.ToBase64PathSafe()}/Buffer/Entries/{Id.ToBase64PathSafe()}.json";
    
    /// <summary>
    /// The path to the directory that contains buffered attached files.
    /// </summary>
    public string BufferFileBasePath => $"../Database/{TableName.ToBase64PathSafe()}/Buffer/Files/{Id.ToBase64PathSafe()}";
    
    /// <summary>
    /// Returns the path to the attached file with the given file ID.
    /// </summary>
    public string GetFilePath(string fileId) => $"{FileBasePath}/{fileId.ToBase64PathSafe()}";
    
    /// <summary>
    /// Returns the path to the trashed attached file with the given file ID.
    /// </summary>
    public string GetTrashFilePath(string fileId) => $"{TrashFileBasePath}/{fileId.ToBase64PathSafe()}";
    
    /// <summary>
    /// Returns the path to the buffered attached file with the given file ID.
    /// </summary>
    public string GetBufferFilePath(string fileId) => $"{BufferFileBasePath}/{fileId.ToBase64PathSafe()}";
    
    /// <summary>
    /// Sets the value to the given serialized value.<br/>
    /// If the basic data object is supplied, it will be used instead of deserializing the given bytes.
    /// </summary>
    public abstract void SetBytes(byte[] serialized, MinimalTableValue? entryInfo = null);
    
    /// <summary>
    /// Returns the serialized value, while preferring the version in the cache.
    /// </summary>
    public byte[] GetBytes()
        => SerializedValue?.Data ?? File.ReadAllBytes(Path);
    
    /// <summary>
    /// Asynchronously returns the bytes of the file with the given file ID. 
    /// </summary>
    public Task<byte[]> GetFileBytes(string fileId)
        => File.ReadAllBytesAsync(GetFilePath(fileId));
    
    /// <summary>
    /// Creates all directories needed to write attached files.
    /// </summary>
    public void CreateFileDirectories()
    {
        Directory.CreateDirectory(FileBasePath);
        Directory.CreateDirectory(TrashFileBasePath);
        Directory.CreateDirectory(BufferFileBasePath);
    }
    
    /// <summary>
    /// Deletes all directories needed to write attached files, if they exist.
    /// </summary>
    public void DeleteFileDirectories()
    {
        if (Directory.Exists(FileBasePath))
            Directory.Delete(FileBasePath, true);
        if (Directory.Exists(TrashFileBasePath))
            Directory.Delete(TrashFileBasePath, true);
        if (Directory.Exists(BufferFileBasePath))
            Directory.Delete(BufferFileBasePath, true);
    }
    
    /// <summary>
    /// Deletes all files needed to write the entry's value, if they exist.
    /// </summary>
    public void DeleteEntryFiles()
    {
        if (File.Exists(Path))
            File.Delete(Path);
        if (File.Exists(TrashPath))
            File.Delete(TrashPath);
        if (File.Exists(BufferPath))
            File.Delete(BufferPath);
    }
}