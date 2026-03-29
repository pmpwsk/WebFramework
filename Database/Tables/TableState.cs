using System.Runtime.Serialization;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the state of a table.
/// </summary>
[DataContract]
public class TableState(ulong typeIteration)
{
    /// <summary>
    /// The version of the table's stored type, used for migration.
    /// </summary>
    [DataMember]
    public ulong TypeIteration = typeIteration;
}