using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace uwap.WebFramework.Database;

public delegate R GetFromFileDelegate<out R>(string fileId);

/// <summary>
/// Abstract class for table values, containing the common data.
/// </summary>
[DataContract]
public abstract class AbstractTableValue(EntryState state)
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
    /// The name of the table the value is stored in, or null if the value hasn't been persisted yet.
    /// </summary>
    public string? TableNameNullable => ContainingEntry?.Table.Name;
    
    /// <summary>
    /// The name of the table the value is stored in. If the value hasn't been persisted yet, an exception is thrown.
    /// </summary>
    public string TableName => TableNameNullable ?? throw new Exception("Containing entry was not set.");
    
    /// <summary>
    /// Moved to <c>State.Timestamp</c>.
    /// </summary>
    [DataMember(EmitDefaultValue = false, Name = "Timestamp")]
    private long TimestampOld = default;

    /// <summary>
    /// Moved to <c>State.Deleted</c>.
    /// </summary>
    [DataMember(EmitDefaultValue = false, Name = "Deleted")]
    private bool DeletedOld = default;

    /// <summary>
    /// Moved to <c>State.Files</c>.
    /// </summary>
    [DataMember(EmitDefaultValue = false, Name = "Files")]
    private Dictionary<string, DatabaseFileData>? FilesOld = default;
    
    /// <summary>
    /// The entry's metadata.
    /// </summary>
    [JsonInclude]
    [DataMember]
    public EntryState State = state;
    
    /// <summary>
    /// Ensures that the fields in <c>AbstractTableValue</c> exist and sets them to default values if they don't.<br/>
    /// This method also migrates the properties to <c>State</c> if necessary.
    /// </summary>
    public void EnsureMinimalTableValue()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (State == null)
        {
            if (FilesOld == null)
            {
                // legacy
                State = new(TimestampOld, DeletedOld, []);
            }
            else
            {
                // no separate state yet
                State = new(TimestampOld, DeletedOld, FilesOld);
                TimestampOld = default;
                DeletedOld = default;
                FilesOld = default;
            }
        }
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
        State.Files[fileId] = new(State.Timestamp, length);
    }
    
    /// <summary>
    /// Returns a list of all file IDs.
    /// </summary>
    /// <returns></returns>
    public List<string> ListFileIds()
        => State.Files.Keys.ToList();
    
    /// <summary>
    /// Returns whether an attached file with the given file ID exists.
    /// </summary>
    public bool ContainsFile(string fileId)
        => State.Files.ContainsKey(fileId);
    
    /// <summary>
    /// Finds the file metadata for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFileInfo(string fileId, [MaybeNullWhen(false)] out DatabaseFileData info)
        => State.Files.TryGetValue(fileId, out info);
    
    /// <summary>
    /// Returns the file metadata for the attached file with the given file ID, or null if no such file exists.
    /// </summary>
    public DatabaseFileData? GetFileInfo(string fileId)
        => State.Files.GetValueOrDefault(fileId);
    
    /// <summary>
    /// Finds the file path for the attached file with the given file ID.
    /// </summary>
    public bool TryGetFilePath(string fileId, [MaybeNullWhen(false)] out string path)
    {
        if (ContainingEntry == null)
            throw new Exception("Containing entry was not set.");
        
        var p = ContainingEntry.GetFilePath(fileId);
        if (State.Files.ContainsKey(fileId) && File.Exists(p))
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