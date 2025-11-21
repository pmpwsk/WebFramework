using System.Collections;

namespace uwap.WebFramework.Tools;

/// <summary>
/// A data structure that represents a set where each item is automatically removed after a specified time.<br/>
/// Setting <c>strict</c> to <c>true</c> enables additional checks so the time limit is exact instead of being a clean-up time. 
/// </summary>
public class ExpiringSet<T>(TimeSpan expiration, bool strict) : IEnumerable<T> where T : notnull
{
    /// <summary>
    /// The time after which an added item is automatically removed.
    /// </summary>
    public readonly TimeSpan Expiration = expiration;
    
    /// <summary>
    /// Whether the time limit should be exact instead of being a clean-up time.
    /// </summary>
    public readonly bool Strict = strict;
    
    /// <summary>
    /// The items and their expiration dates.
    /// </summary>
    private readonly Dictionary<T, DateTime> Items = [];
    
    /// <summary>
    /// Adds the given item to the set. If it already exists, its expiration will be reset.
    /// </summary>
    public void Add(T value)
    {
        lock (Items)
        {
            var date = DateTime.UtcNow.Add(Expiration);
            Items[value] = date;
            _ = Task.Run(async () =>
            {
                await Task.Delay(Expiration);
                lock (Items)
                    if (Items.TryGetValue(value, out var currentDate) && currentDate == date)
                        Items.Remove(value);
            });
        }
    }
    
    /// <summary>
    /// Removes the given item from the set.
    /// </summary>
    public void Remove(T value)
    {
        lock (Items)
            Items.Remove(value);
    }
    
    /// <summary>
    /// Removes all items from the set.
    /// </summary>
    public void Clear()
    {
        lock (Items)
            Items.Clear();
    }
    
    /// <summary>
    /// Checks whether the set contains the given value.
    /// </summary>
    public bool Contains(T value)
    {
        lock (Items)
            if (Strict)
                return Items.TryGetValue(value, out var date) && date >= DateTime.UtcNow;
            else
                return Items.ContainsKey(value);
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        lock (Items)
            if (Strict)
            {
                var date = DateTime.UtcNow;
                return Items.Where(kv => kv.Value >= date).Select(kv => kv.Key).ToList().GetEnumerator();
            }
            else
                return Items.Keys.ToList().GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}