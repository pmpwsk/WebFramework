using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

/// <summary>
/// Functionality to apply cookie changes to a fixed HTTP context.
/// </summary>
public class ContextCookieWriter(HttpContext context) : ICookieWriter
{
    private readonly HttpContext Context = context;
    
    public Task ApplyAsync(Action<HttpContext> action)
    {
        action(Context);
        return Task.CompletedTask;
    }
}