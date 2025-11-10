namespace uwap.Database;

/// <summary>
/// Table index to find table entries (multiple per key) by their (single) key.
/// </summary>
public class MultipleTableIndex<T, K>(Func<T,K> selector) : AbstractTableIndex<T, K>(selector) where T : AbstractTableValue where K : notnull
{
    /// <summary>
    /// The index for lookups.
    /// </summary>
    private Dictionary<K, HashSet<string>> Index = [];
    
    /// <summary>
    /// The keys that are present in the index.
    /// </summary>
    public IReadOnlyCollection<K> Keys => Index.Keys;
    
    /// <summary>
    /// Returns the list of table values that have the given key.
    /// </summary>
    public IReadOnlyCollection<string> Get(K key)
    {
        Lock.EnterReadLock();
        try
        {
            return Index.TryGetValue(key, out var set) ? [..set] : [];
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
    
    protected override void Add(K key, string id)
    {
        if (Index.TryGetValue(key, out var set))
            set.Add(id);
        else Index[key] = [ id ];
    }

    protected override void Remove(K key, string id)
    {
        if (Index.TryGetValue(key, out var set))
        {
            set.Remove(id);
            if (set.Count == 0)
                Index.Remove(key);
        }
    }
}