using System.Runtime.Serialization;

namespace uwap.WebFramework;

/// <summary>
/// A backed up database.
/// </summary>
[DataContract]
public class BackupState()
{
    /// <summary>
    /// The backed up tables.
    /// </summary>
    [DataMember]
    public Dictionary<string, BackupTable> Tables = [];
}