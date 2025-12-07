using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework;

/// <summary>
/// Manages cookies for an IRequest.
/// </summary>
public class CookieManager
{
    internal readonly Dictionary<string, string> KnownCookies = [];
    
    public CookieManager(HttpContext context)
    {
        foreach (var (key, value) in context.Request.Cookies)
            KnownCookies[key] = KnownCookies.TryGetValue(key, out var oldValue) ? $"{oldValue} {value}" : value;
    }

    /// <summary>
    /// Gets the value of the cookie with the given key from the client.
    /// </summary>
    public string this[string key]
        => TryGetValue(key, out var v) ? v : "";

    /// <summary>
    /// Returns whether the client has a cookie with the given key.
    /// </summary>
    public bool Contains(string key)
        => KnownCookies.ContainsKey(key);

    /// <summary>
    /// Returns the value of the cookie with the given key or null if no such cookie was sent.
    /// </summary>
    public string? TryGet(string key)
        => TryGetValue(key, out var v) ? v : null;

    /// <summary>
    /// Returns whether the request contains a cookie with the given key and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => KnownCookies.TryGetValue(key, out value);
    
    /// <summary>
    /// Lists all request cookies.
    /// </summary>
    public List<KeyValuePair<string, string>> ListAll()
        => KnownCookies.ToList();
}