using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework;

/// <summary>
/// Manages cookies for an IRequest.
/// </summary>
public class CookieManager
{
    /// <summary>
    /// Request cookies.
    /// </summary>
    private readonly IRequestCookieCollection Request;

    /// <summary>
    /// Response cookies.
    /// </summary>
    private readonly IResponseCookies Response;

    /// <summary>
    /// Creates a new object to manage cookies for an IRequest.
    /// </summary>
    public CookieManager(HttpContext context)
    {
        Request = context.Request.Cookies;
        Response = context.Response.Cookies;
    }

    /// <summary>
    /// Asks the client to add a simple session cookie (temporary!) with the given key and value.
    /// </summary>
    public void Add(string key, string value)
        => Response.Append(key, value);

    /// <summary>
    /// Asks the client to add a cookie with the given key, value and options.
    /// </summary>
    public void Add(string key, string value, CookieOptions options)
        => Response.Append(key, value, options);

    /// <summary>
    /// Asks the client to delete the cookie with the given key.
    /// </summary>
    /// <param name="key"></param>
    public void Delete(string key)
        => Response.Delete(key);

    /// <summary>
    /// Gets the value of the cookie with the given key from the client.
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (Request.ContainsKey(key)) return Request[key] ?? "";
            else return "";
        }
    }

    /// <summary>
    /// Returns whether the client has a cookie with the given key.
    /// </summary>
    public bool Contains(string key)
        => Request.ContainsKey(key) && Request[key] != null;

    /// <summary>
    /// Returns the value of the cookie with the given key or null if no such cookie was sent.
    /// </summary>
    public string? TryGet(string key) => Request.TryGetValue(key, out var value) ? ((string?)value) ?? "" : null;

    /// <summary>
    /// Returns whether the request contains a cookie with the given key and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (Request.TryGetValue(key, out var v))
        {
            value = ((string?)v) ?? "";
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}