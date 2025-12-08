using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

/// <summary>
/// Manages cookies for an IRequest.
/// </summary>
public class CookieWriter(CookieManager manager, HttpContext context)
{
    public readonly CookieManager Manager = manager;
    
    public readonly HttpContext HttpContext = context;

    /// <summary>
    /// Asks the client to add a simple session cookie (temporary!) with the given key and value.
    /// </summary>
    public void Set(string key, string value)
    {
        HttpContext.Response.Cookies.Append(key, value);
        Manager.KnownCookies[key] = value;
    }

    /// <summary>
    /// Asks the client to add a cookie with the given key, value and options.
    /// </summary>
    public void Set(string key, string value, CookieOptions options)
    {
        HttpContext.Response.Cookies.Append(key, value, options);
        Manager.KnownCookies[key] = value;
    }

    /// <summary>
    /// Asks the client to delete the cookie with the given key.
    /// </summary>
    public void Delete(string key)
    {
        HttpContext.Response.Cookies.Delete(key);
        Manager.KnownCookies.Remove(key);
    }

    /// <summary>
    /// Asks the client to delete the cookie with the given key and options.
    /// </summary>
    public void Delete(string key, CookieOptions options)
    {
        HttpContext.Response.Cookies.Delete(key, options);
        Manager.KnownCookies.Remove(key);
    }
}