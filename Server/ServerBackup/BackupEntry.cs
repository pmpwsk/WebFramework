using System.Runtime.Serialization;

namespace uwap.WebFramework;

/// <summary>
/// A backed up database entry.
/// </summary>
[DataContract]
public class BackupEntry(string origin, long timestamp)
{
    /// <summary>
    /// The backup ID of the backup the entry should be pulled from.
    /// </summary>
    [DataMember]
    public string Origin = origin;
    
    /// <summary>
    /// The timestamp ticks of the last write time.
    /// </summary>
    [DataMember]
    public long Timestamp = timestamp;
    
    /// <summary>
    /// Backup information about the entry's attached files.
    /// </summary>
    [DataMember]
    public Dictionary<string, BackupFile> Files = [];
}