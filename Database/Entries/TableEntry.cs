using System.Runtime.Serialization;
using uwap.WebFramework;

namespace uwap.Database;

public delegate void ValueChangedHandler<in T>(T? oldValue, T? newValue);

/// <summary>
/// Contains the table entry functionality.
/// </summary>
public class TableEntry<T>(string tableName, string id, byte[] serialized)
    : AbstractTableEntry(tableName, id, serialized)
    where T : AbstractTableValue
{
    /// <summary>
    /// The event that is raised when the entry's value has changed.
    /// </summary>
    public readonly SubscriberContainer<ValueChangedHandler<T>> ValueChanged = new();
    
    /// <summary>
    /// Returns the entry's deserialized value.
    /// </summary>
    /// <returns></returns>
    public T? Deserialize()
    {
        try
        {
            if (EntryInfo.Deleted)
                return null;
        
            var value = Serialization.Deserialize<T>(TableName, Id, GetBytes());
            if (value != null)
                value.ContainingEntry = this;
            return value;
        }
        catch
        {
            return null;
        }
    }
    
    public override void SetBytes(byte[] serialized, MinimalTableValue? entryInfo = null)
    {
        if (File.Exists(TrashPath))
            File.Delete(TrashPath);
        if (File.Exists(Path))
            File.Move(Path, TrashPath);
        if (File.Exists(BufferPath))
            File.Delete(BufferPath);
        File.WriteAllBytes(BufferPath, serialized);
        File.Move(BufferPath, Path);
        if (File.Exists(TrashPath))
            File.Delete(TrashPath);
        SerializedValue = Server.Config.Database.CacheEntries ? new(serialized) : null;
        EntryInfo = entryInfo ?? Serialization.Deserialize<MinimalTableValue>(TableName, Id, serialized) ?? throw new SerializationException();
    }

    /// <summary>
    /// Notifies event subscribers that the value of the entry was changed.
    /// </summary>
    public void CallChangedEvent(T? oldValue, T? newValue)
        => ValueChanged.Invoke(s => s(oldValue, newValue), _ => {});
}