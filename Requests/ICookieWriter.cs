using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

/// <summary>
/// Generic type to apply cookie changes.
/// </summary>
public interface ICookieWriter
{
    /// <summary>
    /// Applies the provided action to an appropriate HTTP context.
    /// </summary>
    public Task ApplyAsync(Action<HttpContext> action);
}