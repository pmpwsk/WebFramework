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

    public abstract IEnumerable<AbstractWatchedContainer?> RenderedContainers { get; }

    public readonly SubscriberContainer<Action> Disposing = new();

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        Disposing.InvokeWithSyncCaller(s => s(), null).GetAwaiter().GetResult();
        Disposing.Dispose();
    }
}