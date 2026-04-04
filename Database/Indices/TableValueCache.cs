using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Table index to access a property by the entry's ID without properly loading the entry.
/// </summary>
public abstract class TableValueCache<T, V>(Func<T,V> selector) : ITableIndex<T> where T : AbstractTableValue
{
    /// <summary>
    /// The function to select the value from a table value.
    /// </summary>
    private readonly Func<T,V> Selector = selector;
    
    /// <summary>
    /// The lock to use when updating the index.
    /// </summary>
    protected AsyncReaderWriterLock Lock = new();
    
    /// <summary>
    /// The index for lookups.
    /// </summary>
    private Dictionary<string, V> Index = [];

    public async Task UpdateAsync(string id, T? value)
    {
        await using var h = await Lock.WaitWriteAsync();
        
        if (value != null)
            Index[id] = Selector(value);
        else
            Index.Remove(id);
    }
    
    /// <summary>
    /// Returns the property value of the entry with the given ID.
    /// </summary>
    public async Task<V> GetAsync(string id)
    {
        await using var h = await Lock.WaitReadAsync();
        
        return Index.TryGetValue(id, out var result) ? result : throw new Exception($"The index does not contain ID '{id}'");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Lock.Dispose();
    }
}