using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Database;

public delegate R GetFromFileDelegate<out R>(string fileId);

/// <summary>
/// Abstract class for table values, containing the common data.
/// </summary>
[DataContract]
public abstract class AbstractTableValue
{
    /// <summary>
    /// The entry the value is stored in, or null if no entry was created for it yet.
    /// </summary>
    public AbstractTableEntry? ContainingEntry { get; internal set; } = null;

    /// <summary>
    /// The ID of the entry the value is stored in, or null if no entry was created for it yet.
    /// </summary>
    public string? IdNullable => ContainingEntry?.Id;
    
    /// <summary>
    /// The ID of the entry the value is stored in. If no such entry was created yet, an exception is thrown.
    /// </summary>
    public string Id => IdNullable ?? throw new Exception("Containing entry was not set.");
    
    /// <summary>
    /// The timestamp ticks of the last write time.
    /// </summary>
    [DataMember]
    public long Timestamp { get; internal set; } = 0;

    /// <summary>
    /// Whether the entry has been marked as deleted.
    /// </summary>
    [DataMember]
    public bool Deleted { get; internal set; } = false;

    /// <summary>
    /// Information about the attached files, indexed by their file IDs.
    /// </summary>
    [DataMember]
    internal Dictionary<string, DatabaseFileData> Files = [];
    
    /// <summary>
    /// Ensures that the fields in <c>AbstractTableValue</c> exist and sets them to default values if they don't.<br/>
    /// This should be called in migrations from type iteration '0'.<br/>
    /// Returns <c>true</c> if the value has been changed, otherwise <c>false</c>.
    /// </summary>
    public bool EnsureMinimalTableValue()
    {
        bool dirty = false;
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Files == null)
        {
            dirty = true;
            Files = [];
        }
        
        return dirty;
    }
    
    /// <summary>
    /// Migrates a file from the legacy database to the new database.
    /// </summary>
    public void MigrateLegacyFile(AbstractTable table, string id, string fileId, string path)
    {
        var length = new FileInfo(path).Length;
        var fileBasePath = $"../Database/{table.Name.ToBase64PathSafe()}/Files/{id.ToBase64PathSafe()}";
        Directory.CreateDirectory(fileBasePath);
        File.Move(path, $"{fileBasePath}/{fileId.ToBase64PathSafe()}");
        Files[fileId] = new(Timestamp, length);
    }
    
    /// <summary>
    /// Returns a list of all file IDs.
    /// </summary>
    /// <returns></returns>
    public List<string> ListFileIds()
        => Files.Keys.ToList();
    
    /// <summary>
    /// Returns whether an attached file with the given file ID exists.
    /// </summary>
    public bool ContainsFile(string fileId)
        => Files.ContainsKey(fileId);
    
    /// <summary>
    /// Finds the file metadata for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFileInfo(string fileId, [MaybeNullWhen(false)] out DatabaseFileData info)
        => Files.TryGetValue(fileId, out info);
    
    /// <summary>
    /// Returns the file metadata for the attached file with the given file ID, or null if no such file exists.
    /// </summary>
    public DatabaseFileData? GetFileInfo(string fileId)
        => Files.GetValueOrDefault(fileId);
    
    /// <summary>
    /// Finds the file path for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFilePath(string fileId, [MaybeNullWhen(false)] out string path)
    {
        if (ContainingEntry == null)
            throw new Exception("Containing entry was not set.");
        
        var p = ContainingEntry.GetFilePath(fileId);
        if (Files.ContainsKey(fileId) && File.Exists(p))
        {
            path = p;
            return true;
        }
        else
        {
            path = null;
            return false;
        }
    }
    
    /// <summary>
    /// Finds the file content as bytes for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFileBytes(string fileId, [MaybeNullWhen(false)] out byte[] content)
        => TryGetFromFile(fileId, File.ReadAllBytes, out content);
    
    /// <summary>
    /// Finds the file content as text for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFileText(string fileId, [MaybeNullWhen(false)] out string content)
        => TryGetFromFile(fileId, File.ReadAllText, out content);
    
    /// <summary>
    /// Finds something for the attached file with the given file ID using the given getter.
    /// </summary>
    public bool TryGetFromFile<R>(string fileId, GetFromFileDelegate<R> getter, [MaybeNullWhen(false)] out R result)
    {
        if (TryGetFilePath(fileId, out var path))
        {
            result = getter(path);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
    
    /// <summary>
    /// Returns the file content as bytes for the attached file with the given file ID, or null if no such file exists.
    /// </summary>
    public byte[]? GetFileBytes(string fileId)
        => GetFromFile(fileId, File.ReadAllBytes);
    
    /// <summary>
    /// Returns the file content as text for the attached file with the given file ID, or null if no such file exists.
    /// </summary>
    public string? GetFileText(string fileId)
        => GetFromFile(fileId, File.ReadAllText);
    
    /// <summary>
    /// Returns something for the attached file with the given file ID using the given getter, or null if no such file exists.
    /// </summary>
    public R? GetFromFile<R>(string fileId, GetFromFileDelegate<R> getter)
        => TryGetFilePath(fileId, out var path) ? getter(path) : default;
    
    /// <summary>
    /// Adds a file deletion action for the given file ID to the given file action list if the file exists.
    /// </summary>
    public bool DeleteFileIfExists(string fileId, List<IFileAction> fileActions)
    {
        if (ContainsFile(fileId))
        {
            fileActions.Add(new DeleteFileAction(fileId));
            return true;
        }
        return false;
    }
}