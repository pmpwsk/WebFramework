using System.Runtime.Serialization;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the minimal common data every table value has.<br/>
/// This class is used to deserialize values without the type-specific parts and to serialize deleted entries.
/// </summary>
[DataContract]
public sealed class MinimalTableValue : AbstractTableValue
{
}