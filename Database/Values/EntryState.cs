using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the metadata of a table entry.
/// </summary>
[method: JsonConstructor]
[DataContract]
public class EntryState(
    long timestamp,
    bool deleted,
    Dictionary<string, DatabaseFileData> files
)
{
    /// <summary>
    /// Creates an entry state for a new entry.
    /// </summary>
    public static EntryState CreateEmpty()
        => new(0, false, []);
    
    /// <summary>
    /// The timestamp ticks of the last write time.
    /// </summary>
    [JsonInclude]
    [DataMember]
    public long Timestamp { get; internal set; } = timestamp;

    /// <summary>
    /// Whether the entry has been marked as deleted.
    /// </summary>
    [JsonInclude]
    [DataMember]
    public bool Deleted { get; internal set; } = deleted;

    /// <summary>
    /// Information about the attached files, indexed by their file IDs.
    /// </summary>
    [JsonInclude]
    [DataMember]
    internal Dictionary<string, DatabaseFileData> Files = files;
}