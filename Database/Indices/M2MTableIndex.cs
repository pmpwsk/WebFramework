using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Table index to find table entries (multiple per key) by any of their (multiple) keys.
/// </summary>
public class M2MTableIndex<T, K>(Func<T,List<K>> selector) : ITableIndex<T> where T : AbstractTableValue where K : notnull
{
    /// <summary>
    /// The function to select the list of keys from a table value.
    /// </summary>
    private readonly Func<T,List<K>> Selector = selector;
    
    /// <summary>
    /// The lock to use when updating the index.
    /// </summary>
    protected ReaderWriterLockSlim Lock = new();
    
    /// <summary>
    /// The index connecting entries to their keys for easy deletion.
    /// </summary>
    protected Dictionary<string,HashSet<K>> ReverseIndex = [];
    
    /// <summary>
    /// The index for lookups.
    /// </summary>
    private Dictionary<K, HashSet<string>> Index = [];
    
    /// <summary>
    /// The keys that are present in the index.
    /// </summary>
    public IReadOnlyCollection<K> Keys => Index.Keys;

    public void Update(string id, T? value)
    {
        Lock.EnterWriteLock();
        try
        {
            if (value != null)
            {
                var keys = Selector(value);
                var reverseSet = ReverseIndex.GetValueOrAdd(id, () => []);
                foreach (var key in keys)
                    if (!reverseSet.Contains(key))
                    {
                        var indexSet = Index.GetValueOrAdd(key, () => []);
                        indexSet.Add(id);
                        
                        reverseSet.Add(key);
                    }
                
                foreach (var reverseKey in reverseSet.ToList())
                    if (!keys.Contains(reverseKey))
                    {
                        if (Index.TryGetValue(reverseKey, out var indexSet))
                        {
                            indexSet.Remove(id);
                            if (indexSet.Count == 0)
                                Index.Remove(reverseKey);
                        }
                        
                        reverseSet.Remove(reverseKey);                        
                    }
            }
            else
            {
                if (!ReverseIndex.TryGetValue(id, out var reverseKeys))
                    return;
                
                foreach (var reverseKey in reverseKeys)
                    if (Index.TryGetValue(reverseKey, out var indexSet))
                    {
                        indexSet.Remove(id);
                        if (indexSet.Count == 0)
                            Index.Remove(reverseKey);
                    }
                
                ReverseIndex.Remove(id);
            }
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Returns the list of table values that have the given key in their list of keys.
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
}