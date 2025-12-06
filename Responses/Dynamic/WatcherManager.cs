using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract page that can have a change watcher.
/// </summary>
public static class WatcherManager
{
    public static readonly IResponse RejectResponse
        = new SingleEventMessageResponse(JsonSerializer.Serialize(new { type = "Reload" }));
    
    /// <summary>
    /// The currently watched pages, indexed by their watcher IDs.
    /// </summary>
    private static Dictionary<string, AbstractWatchablePage> WatchedPages = [];
    
    /// <summary>
    /// Creates and saves a new watcher for the given page, while starting its expiration timeout.
    /// </summary>
    public static ChangeWatcher CreateWatcher(AbstractWatchablePage page)
    {
        lock (WatchedPages)
        {
            var id = Parsers.RandomString(64, WatchedPages.Keys);
            var watcher = new ChangeWatcher(id);
            WatchedPages[id] = page;
            return watcher;
        }
    }
    
    /// <summary>
    /// Deletes the given watcher.
    /// </summary>
    public static void DeleteWatcher(ChangeWatcher watcher)
    {
        lock (WatchedPages)
        {
            if (WatchedPages.GetValueOrDefault(watcher.Id)?.ChangeWatcher != watcher)
                return;
            
            WatchedPages.Remove(watcher.Id);
            watcher.ExpirationToken.Cancel();
        }
    }
    
    /// <summary>
    /// Attempts to find the page with the given watcher ID.
    /// </summary>
    public static bool TryGetPage(string id, [MaybeNullWhen(false)] out AbstractWatchablePage page)
    {
        lock (WatchedPages)
            return WatchedPages.TryGetValue(id, out page);
    }
    
    /// <summary>
    /// Attempts to find the change watcher with the given watcher ID.
    /// </summary>
    public static bool TryGetWatcher(string id, [MaybeNullWhen(false)] out ChangeWatcher watcher)
    {
        if (TryGetPage(id, out var page) && page.ChangeWatcher != null)
        {
            watcher = page.ChangeWatcher;
            return true;
        }
        else
        {
            watcher = null;
            return false;
        }
    }
}