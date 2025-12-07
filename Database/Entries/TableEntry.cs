using System.Runtime.Serialization;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

public delegate void ValueChangedHandler<in T>(T? oldValue, T? newValue);

/// <summary>
/// Contains the table entry functionality.
/// </summary>
public class TableEntry<T>(Table<T> table, string id, byte[] serialized)
    : AbstractTableEntry(table, id, serialized)
    where T : AbstractTableValue
{
    /// <summary>
    /// The event that is raised when the entry's value has changed.
    /// </summary>
    public readonly SubscriberContainer<ValueChangedHandler<T>> ValueChanged = new();
    
    public override Table<T> Table { get; } = table;
    
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
        
            var value = Serialization.Deserialize<T>(Table, Id, GetBytes());
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
        EntryInfo = entryInfo ?? Serialization.Deserialize<MinimalTableValue>(Table, Id, serialized) ?? throw new SerializationException();
    }

    /// <summary>
    /// Notifies event subscribers that the value of the entry was changed.
    /// </summary>
    public Task CallChangedEventAsync(T? oldValue, T? newValue)
        => ValueChanged.InvokeWithSyncCaller(s => s(oldValue, newValue), _ => {});

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        ValueChanged.Dispose();
    }
}