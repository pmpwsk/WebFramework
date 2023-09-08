using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace uwap.WebFramework;

/// <summary>
/// Manages the query of an IRequest.
/// </summary>
public class QueryManager
{
    /// <summary>
    /// The query object.
    /// </summary>
    private IQueryCollection Query;

    /// <summary>
    /// Creates a new object to manage the query of an IRequest.
    /// </summary>
    public QueryManager(IQueryCollection query)
    {
        Query = query;
    }

    /// <summary>
    /// Gets the value of the query entry with the given key. If no such query entry exists, an exception is thrown.
    /// </summary>
    public string this[string key]
    {
        get
        {
            string? value = Query[key];
            if (value == null) throw new ArgumentException("Query does not contain the provided key.");
            return value;
        }
    }

    /// <summary>
    /// Whether the query contains an entry with the given key.
    /// </summary>
    public bool ContainsKey(string key) => Query.ContainsKey(key);

    /// <summary>
    /// Whether the query contains entries for all of the given keys.
    /// </summary>
    public bool ContainsKeys(params string[] keys) => keys.All(Query.ContainsKey);

    /// <summary>
    /// Returns the value of the query entry with the given key or null if no such entry exists.
    /// </summary>
    public string? TryGet(string key) => Query.TryGetValue(key, out var value) ? ((string?)value)??"" : null;

    /// <summary>
    /// Returns whether the query contains an entry with the given key and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (Query.TryGetValue(key, out var v))
        {
            value = ((string?)v)??"";
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}