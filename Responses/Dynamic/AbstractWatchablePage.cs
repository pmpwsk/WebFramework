using uwap.WebFramework.Database;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract page that can have a change watcher.
/// </summary>
public abstract class AbstractWatchablePage : AbstractTextResponse, IWatchedParent
{
    /// <summary>
    /// The request that initially created this page.
    /// </summary>
    public readonly Request Request;
    
    /// <summary>
    /// Whether the page will be watched for changes.
    /// </summary>
    public readonly bool Dynamic;
    
    public ChangeWatcher? ChangeWatcher { get; internal set; }

    public readonly string? WatchedUrl;

    public List<AbstractWatchedContainer?> RenderedContainers { get; }

    public readonly SubscriberContainer<Action> Disposing;
    
    private readonly CancellationTokenSource CancellationOnDisposeSource;
    public readonly CancellationToken CancellationOnDispose;

    /// <summary>
    /// An abstract page that can have a change watcher.
    /// </summary>
    protected AbstractWatchablePage(Request req, bool dynamic)
    {
        Request = req;
        Dynamic = dynamic;
        WatchedUrl = dynamic ? req.ProtoHostPathQuery : null;
        Disposing = new();
        RenderedContainers = [];
        CancellationOnDisposeSource = new();
        CancellationOnDispose = CancellationOnDisposeSource.Token;
    }

    public WatchedElement? FindByPath(string[] path)
    {
        var containers = RenderedContainers;
        for (int i = 0; i < path.Length; i++)
        {
            var child = containers
                .WhereNotNull()
                .SelectMany(c => c)
                .OfType<WatchedElement>()
                .FirstOrDefault(e => e.SystemId == path[i]);
            if (child == null)
                return null;
            if (i == path.Length - 1)
                return child;
            containers = child.RenderedContainers;
        }

        return null;
    }
    
    /// <summary>
    /// Sets the page to internally reload when the entry is changed.
    /// </summary>
    public async Task ReloadOnChangeAsync<T>(T value) where T : AbstractTableValue
    {
        if (value.ContainingEntry is TableEntry<T> entry)
        {
            ValueChangedHandler<T> updater = (_, _) => Reload();
            
            await entry.ValueChanged.RegisterAsync(updater);
            await Disposing.RegisterAsync(() => entry.ValueChanged.UnregisterAsync(updater).GetAwaiter().GetResult());
        }
    }
    
    /// <summary>
    /// Reloads a dynamic page internally.
    /// </summary>
    public void Reload()
        => ChangeWatcher?.InternalReload();
    
    /// <summary>
    /// Reloads a dynamic page using the browser navigation.
    /// </summary>
    public void BrowserReload()
        => ChangeWatcher?.BrowserReload();
    
    /// <summary>
    /// Shows a browser alert on a dynamic page.
    /// </summary>
    public void BrowserAlert(string message)
        => ChangeWatcher?.BrowserAlert(message);
    
    /// <summary>
    /// Directs the dynamic page to the new location.
    /// </summary>
    public void Navigate(string location)
        => ChangeWatcher?.Navigate(location);

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        Disposing.InvokeWithSyncCaller(s => s(), null).GetAwaiter().GetResult();
        CancellationOnDisposeSource.Cancel();
        Disposing.Dispose();
    }
}