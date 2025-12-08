using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract page that can have a change watcher.
/// </summary>
public static class WatcherManager
{
    /// <summary>
    /// The currently watched pages, indexed by their watcher IDs.
    /// </summary>
    private static readonly Dictionary<string, AbstractWatchablePage> WatchedPages = [];
    
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
            page.ChangeWatcher = watcher;
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

            if (WatchedPages.Remove(watcher.Id, out var page))
                page.Dispose();
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
}