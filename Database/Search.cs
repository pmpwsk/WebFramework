namespace uwap.Database;

/// <summary>
/// Filters and sorts elements.
/// </summary>
/// <typeparam name="T">The type of elements to work with.</typeparam>
public class Search<T> where T : notnull
{
    /// <summary>
    /// The dictionary containing every element (key) that is currently being worked with along with their assigned value (value) for sorting.
    /// </summary>
    private readonly Dictionary<T,uint> Items;

    /// <summary>
    /// The query of the current search converted to lowercase characters or null if one hasn't been provided.
    /// </summary>
    private readonly string? Query;

    /// <summary>
    /// The query's individual words (separated by spaces) or an empty array if the query is null.
    /// </summary>
    private readonly string[] QuerySplit;

    /// <summary>
    /// The relevance value for the current filtering. It is lowered by 1 every round and gets stuck on 1.
    /// </summary>
    private uint Relevance;

    /// <summary>
    /// Creates a new object to filter and sort the given elements.<br/>
    /// If a query is provided, the elements will first be sorted by how closely they match it, ones that don't match it at all will be excluded.
    /// </summary>
    /// <param name="items">The elements to be filtered and sorted.</param>
    /// <param name="query">The query to search for (not case sensitive) or null.</param>
    /// <param name="relevanceStart">The initial value for the relevance of filters. It is lowered by 1 every round and gets stuck on 1. Default: 1000</param>
    public Search(IEnumerable<T> items, string? query, uint relevanceStart = 1000)
    {
        if (relevanceStart <= 0)
            throw new ArgumentOutOfRangeException("'relevanceStart' should be positive.");
        Relevance = relevanceStart;

        if (query == null)
        {
            Items = items.ToDictionary(x => x, x => 1u);
            Query = null;
            QuerySplit = Array.Empty<string>();
        }
        else
        {
            Items = items.ToDictionary(x => x, x => 0u); 
            Query = query.ToLower();
            QuerySplit = Query.Split(' ');
        }
    }

    /// <summary>
    /// Adds relevance to every element depending on how much the result of the given function applied to it matches the query.
    /// </summary>
    /// <param name="func">The function to get a string (not case sensitive) out of each element to compare that to the query.</param>
    public void Find(Func<T,string> func)
    {
        if (Query == null) return;

        Dictionary<T, string> values = Items.Keys.ToDictionary(x => x, x => func(x).ToLower());

        foreach (var r in values)
        {
            if (r.Value.StartsWith(Query))
                Items[r.Key] += Relevance;
            if (r.Value.Contains(Query))
                Items[r.Key] += Relevance;
            if (QuerySplit.Any(r.Value.StartsWith))
                Items[r.Key] += Relevance;
            if (QuerySplit.Any(r.Value.Contains))
                Items[r.Key] += Relevance;
        }

        if (Relevance > 1) Relevance--;
    }

    /// <summary>
    /// Adds relevance to every element depending on how much any of the strings in the result of the given function applied to it match the query.
    /// </summary>
    /// <param name="func">The function to get multiple strings out of an element to compare those to the query.</param>
    public void Find(Func<T,IEnumerable<string>> func)
    {
        if (Query == null) return;

        Dictionary<T, IEnumerable<string>> values = Items.Keys.ToDictionary(x => x, x => func(x).Select(y => y.ToLower()));

        foreach (var r in values)
        {
            if (r.Value.Any(x => x.StartsWith(Query)))
                Items[r.Key] += Relevance;
            if (r.Value.Any(x => x.Contains(Query)))
                Items[r.Key] += Relevance;
            if (r.Value.Any(x => QuerySplit.Any(q => x.StartsWith(q))))
                Items[r.Key] += Relevance;
            if (r.Value.Any(x => QuerySplit.Any(q => x.Contains(q))))
                Items[r.Key] += Relevance;
        }
        
        if (Relevance > 1) Relevance--;
    }

    /// <summary>
    /// Sorts (ascending order) the elements that match the query (if one has been provided) by how much they match it and then by each additional function that has been provided in order and returns the sorted set of elements..
    /// </summary>
    /// <param name="funcs">The additional functions to sort the matching elements by.</param>
    public IEnumerable<T> Sort(params Func<T, IComparable>[] funcs) => Sort(false, funcs);

    /// <summary>
    /// Sorts (descending order) the elements that match the query (if one has been provided) by how much they match it and then by each additional function that has been provided in order and returns the sorted set of elements.
    /// </summary>
    /// <param name="funcs">The additional functions to sort the matching elements by.</param>
    public IEnumerable<T> SortDesc(params Func<T, IComparable>[] funcs) => Sort(true, funcs);

    /// <summary>
    /// Sorts the elements that match the query (if one has been provided) by how much they match it and then by each additional function that has been provided in order and returns the sorted set of elements.
    /// </summary>
    /// <param name="descending">true if the elements should be sorted in ascending order, false for descending order.</param>
    /// <param name="funcs">The additional functions to sort the matching elements by.</param>
    private IEnumerable<T> Sort(bool descending, params Func<T,IComparable>[] funcs)
    {
        IOrderedEnumerable<T> result = Items.Where(x => x.Value != 0).Select(x => x.Key).OrderByDescending(x => Items[x]);
        foreach (var func in funcs)
        {
            if (descending) result = result.ThenByDescending(func);
            else result = result.ThenBy(func);
        }
        return result;
    }
}