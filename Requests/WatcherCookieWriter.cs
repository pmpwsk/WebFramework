using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework;

/// <summary>
/// Functionality to apply cookie changes to a change watcher.
/// </summary>
public class WatcherCookieWriter(ChangeWatcher watcher) : ICookieWriter
{
    private readonly ChangeWatcher Watcher = watcher;
    
    public async Task ApplyAsync(Action<HttpContext> action)
    {
        await DynamicCookieActionManager.OpenAndWaitAsync(Watcher, action);
    }
}