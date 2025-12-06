using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract page that can have a change watcher.
/// </summary>
public abstract class AbstractWatchablePage(bool dynamic) : AbstractTextResponse, IWatchedParent
{
    /// <summary>
    /// Whether the page will be watched for changes.
    /// </summary>
    public readonly bool Dynamic = dynamic;
    
    public ChangeWatcher? ChangeWatcher { get; private set; }

    public override Task Respond(Request req, HttpContext context)
    {
        if (Dynamic)
            ChangeWatcher = WatcherManager.CreateWatcher(this);
        return base.Respond(req, context);
    }

    public abstract IEnumerable<AbstractWatchedContainer?> RenderedContainers { get; }
}