using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework;

/// <summary>
/// Manages cookies for an IRequest.
/// </summary>
public class CookieManager
{
    /// <summary>
    /// The currently known cookie values. 
    /// </summary>
    private readonly ConcurrentDictionary<string, string> KnownCookies = [];
    
    /// <summary>
    /// The cookie writer to use.
    /// </summary>
    private ICookieWriter? Writer = null;
    
    /// <summary>
    /// Creates a new cookie manager for the given context, while enabling writing to that context.
    /// </summary>
    public CookieManager(HttpContext context)
    {
        foreach (var (key, value) in context.Request.Cookies)
            KnownCookies[key] = KnownCookies.TryGetValue(key, out var oldValue) ? $"{oldValue} {value}" : value;
        
        UseContextWriting(context);
    }
    
    /// <summary>
    /// Uses the same known cookies from the existing cookie manager, writing is disabled.
    /// </summary>
    public CookieManager(CookieManager manager)
    {
        KnownCookies = manager.KnownCookies;
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
        => [.. KnownCookies];
    
    /// <summary>
    /// Removes the cookie writer.
    /// </summary>
    public void DisableWriting()
        => Writer = null;
    
    /// <summary>
    /// Sets the cookie writer to write to the provided HTTP context.
    /// </summary>
    public void UseContextWriting(HttpContext context)
        => Writer = new ContextCookieWriter(context);
    
    /// <summary>
    /// Whether writing is supported.
    /// </summary>
    public bool CanWrite
        => Writer != null;
    
    /// <summary>
    /// Applies the provided cookie action.<br/>
    /// If cookie writing is not supported, an exception is thrown.
    /// </summary>
    private async Task ApplyAsync(Action<HttpContext> action)
    {
        if (Writer == null)
            throw new Exception("This request does not support cookie writing.");
        
        await Writer.ApplyAsync(action);
    }

    /// <summary>
    /// Asks the client to add a simple session cookie (temporary!) with the given key and value.
    /// </summary>
    public Task SetAsync(string key, string value)
        => ApplyAsync(
            context =>
            {
                context.Response.Cookies.Append(key, value);
                KnownCookies[key] = value;
            }
        );

    /// <summary>
    /// Asks the client to add a cookie with the given key, value and options.
    /// </summary>
    public Task SetAsync(string key, string value, CookieOptions options)
        => ApplyAsync(
            context =>
            {
                context.Response.Cookies.Append(key, value, options);
                KnownCookies[key] = value;
            }
        );

    /// <summary>
    /// Asks the client to delete the cookie with the given key.
    /// </summary>
    public Task DeleteAsync(string key)
        => ApplyAsync(
            context =>
            {
                context.Response.Cookies.Delete(key);
                KnownCookies.Remove(key, out _);
            }
        );

    /// <summary>
    /// Asks the client to delete the cookie with the given key and options.
    /// </summary>
    public Task DeleteAsync(string key, CookieOptions options)
        => ApplyAsync(
            context =>
            {
                context.Response.Cookies.Delete(key, options);
                KnownCookies.Remove(key, out _);
            }
        );
}