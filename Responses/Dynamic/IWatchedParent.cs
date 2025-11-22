namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A generic parent that provides a list of rendered containers and a watcher object.
/// </summary>
public interface IWatchedParent
{
    /// <summary>
    /// The watched content containers to render.
    /// </summary>
    public IEnumerable<AbstractWatchedContainer?> RenderedContainers { get; }
    
    /// <summary>
    /// The watcher to notify of any changes.
    /// </summary>
    public ChangeWatcher? ChangeWatcher { get; }
}