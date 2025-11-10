using System.Runtime.Serialization;

namespace uwap.WebFramework;

/// <summary>
/// A backed up database file.
/// </summary>
[DataContract]
public class BackupFile(string origin, long timestamp)
{
    /// <summary>
    /// The backup ID of the backup the file should be pulled from.
    /// </summary>
    [DataMember]
    public string Origin = origin;

    /// <summary>
    /// The timestamp ticks of the last write time.
    /// </summary>
    [DataMember]
    public long Timestamp = timestamp;
}