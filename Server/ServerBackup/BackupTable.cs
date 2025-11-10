using System.Runtime.Serialization;

namespace uwap.WebFramework;

/// <summary>
/// A backed up database table.
/// </summary>
[DataContract]
public class BackupTable()
{
    /// <summary>
    /// The backed up table entries.
    /// </summary>
    [DataMember]
    public Dictionary<string, BackupEntry> Entries = [];
}