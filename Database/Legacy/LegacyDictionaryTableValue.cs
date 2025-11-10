using System.Runtime.Serialization;

namespace uwap.Database;

/// <summary>
/// Table value class that contains a string-string dictionary.
/// </summary>
[DataContract]
public class LegacyDictionaryTableValue : ILegacyTableValue
{
    /// <summary>
    /// The string-string dictionary containing the values.
    /// </summary>
    [DataMember]
    private Dictionary<string,string> Data = [];

    /// <summary>
    /// Gets or sets the value for the given key.
    /// </summary>
    public string this[string key]
    {
        get => Data[key];
        set
        {
            Lock();
            Data[key] = value;
            UnlockSave();
        }
    }
}