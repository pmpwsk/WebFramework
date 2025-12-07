using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Abstract table index to find entries by a value located using the given selector.
/// </summary>
public abstract class AbstractTableIndex<T, K>(Func<T,K> selector) : ITableIndex<T> where T : AbstractTableValue where K : notnull
{
    /// <summary>
    /// The function to select the key from a table value.
    /// </summary>
    private readonly Func<T,K> Selector = selector;
    
    /// <summary>
    /// The lock to use when updating the index.
    /// </summary>
    protected AsyncReaderWriterLock Lock = new();
    
    /// <summary>
    /// The index connecting entries to their keys for easy deletion.
    /// </summary>
    protected Dictionary<string,K> ReverseIndex = [];

    /// <summary>
    /// Adds the given entry ID under the given key.
    /// </summary>
    protected abstract void Add(K value, string id);

    /// <summary>
    /// Removes the given entry ID from the given key.
    /// </summary>
    protected abstract void Remove(K value, string id);

    public async Task UpdateAsync(string id, T? value)
    {
        await using var h = await Lock.WaitWriteAsync();
        
        if (value != null)
        {
            var key = Selector(value);
            if (ReverseIndex.TryGetValue(id, out var reverseKey))
                if (reverseKey.Equals(key))
                    return;
                else Remove(reverseKey, id);
            
            Add(key, id);
            ReverseIndex[id] = key;
        }
        else
        {
            if (!ReverseIndex.TryGetValue(id, out var reverseKey))
                return;
            
            Remove(reverseKey, id);
            ReverseIndex.Remove(id);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Lock.Dispose();
    }
}