using System.Runtime.Serialization;

namespace uwap.Database;

/// <summary>
/// Contains information about a file that's attached to a database entry.
/// </summary>
[DataContract]
public class DatabaseFileData(long timestamp, long size)
{
    /// <summary>
    /// The timestamp ticks of the last write time.
    /// </summary>
    [DataMember]
    public long Timestamp { get; internal set; } = timestamp;
    
    /// <summary>
    /// The size of the file.
    /// </summary>
    [DataMember]
    public long Size { get; internal set; } = size;
}