using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract page that can have a change watcher.
/// </summary>
public abstract class AbstractWatchablePage(Request req, bool dynamic) : AbstractTextResponse, IWatchedParent
{
    /// <summary>
    /// Whether the page will be watched for changes.
    /// </summary>
    public readonly bool Dynamic = dynamic;
    
    public ChangeWatcher? ChangeWatcher { get; internal set; }

    public readonly string? WatchedUrl = dynamic ? req.ProtoHostPathQuery : null;

    public List<AbstractWatchedContainer?> RenderedContainers { get; } = [];

    public readonly SubscriberContainer<Action> Disposing = new();

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

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        Disposing.InvokeWithSyncCaller(s => s(), null).GetAwaiter().GetResult();
        Disposing.Dispose();
    }
}