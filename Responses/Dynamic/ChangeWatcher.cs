using System.Text.Json;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to watch and distribute changes to default UI elements.
/// </summary>
public class ChangeWatcher
{
    public string Id;
    
    public Request? Request = null;
    
    public readonly PlannedAction Expiration;
    
    public readonly CancellationTokenSource ExpirationToken = new();
    
    internal ChangeWatcher(string id)
    {
        Id = id;
        Expiration = new PlannedAction(Server.Config.WatcherExpiration, () => WatcherManager.DeleteWatcher(this));
        Expiration.Start();
    }
    
    private void WriteChange(object change)
    {
        var data = JsonSerializer.Serialize(change);
        Request?.EventMessage(data).GetAwaiter().GetResult();
    }
    
    public void Welcome()
    {
        WriteChange(new { type = "Welcome" });
    }
    
    public void WelcomeBack()
    {
        WriteChange(new { type = "WelcomeBack" });
    }
    
    public void AttributeChanged(WatchedElement element, string attributeName)
    {
        var path = element.GetPath();
        if (path == null)
            return;
        var attributeValue = element.GetAttribute(attributeName);
        WriteChange(new { type = "AttributeChanged", path, attributeName, attributeValue });
    }
    
    public void ElementRemoved(WatchedElement child)
    {
        var path = child.GetPath();
        if (path == null)
            return;
        WriteChange(new { type = "ElementRemoved", path });
    }
    
    public void ElementAdded(WatchedElement child, IWatchedParent parent, WatchedElement? predecessor, WatchedElement? successor)
    {
        if (successor != null)
            ElementAddedBefore(child, successor);
        else if (predecessor != null)
            ElementAddedAfter(child, predecessor);
        else if (parent is WatchedElement watchedParent)
            ContentChanged(watchedParent);
    }
    
    public void ElementAddedBefore(WatchedElement child, WatchedElement successor)
    {
        var path = successor.GetPath();
        if (path == null)
            return;
        var html = string.Join("", child.EnumerateChunks());
        WriteChange(new { type = "ElementAddedBefore", path, html });
    }
    
    public void ElementAddedAfter(WatchedElement child, WatchedElement predecessor)
    {
        var path = predecessor.GetPath();
        if (path == null)
            return;
        var html = string.Join("", child.EnumerateChunks());
        WriteChange(new { type = "ElementAddedAfter", path, html });
    }
    
    public void ContentChanged(WatchedElement element)
    {
        var path = element.GetPath();
        if (path == null)
            return;
        var content = string.Join("", element.RenderedContainers.WhereNotNull().SelectMany(c => c.WhereNotNull().SelectMany(e => e.EnumerateChunks())));
        WriteChange(new { type = "ContentChanged", path, content });
    }
}