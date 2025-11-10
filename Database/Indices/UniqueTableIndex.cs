namespace uwap.Database;

/// <summary>
/// Table index to find table entries (one per key) by their (single) key.
/// </summary>
public class UniqueTableIndex<T, K>(Func<T,K> selector) : AbstractTableIndex<T, K>(selector) where T : AbstractTableValue where K : notnull
{
    /// <summary>
    /// The index for lookups.
    /// </summary>
    private Dictionary<K, string> Index = [];
    
    /// <summary>
    /// The keys that are present in the index.
    /// </summary>
    public IReadOnlyCollection<K> Keys => Index.Keys;
    
    /// <summary>
    /// Returns the table value that have the given key, or null if no such table value exists.
    /// </summary>
    public string? Get(K key)
    {
        Lock.EnterReadLock();
        try
        {
            return Index.GetValueOrDefault(key);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
    
    protected override void Add(K key, string id)
    {
        Index[key] = id;
    }

    protected override void Remove(K key, string id)
    {
        Index.Remove(key);
    }
}