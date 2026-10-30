using uwap.WebFramework.Responses.DefaultUI;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to watch and distribute changes to default UI elements.
/// </summary>
public class ChangeWatcher
{
    public string Id;
    
    public SocketResponse? Socket = null;

    private readonly Queue<object> WaitingChanges = [];

    private bool Loaded = false;
    
    internal ChangeWatcher(string id)
    {
        Id = id;
    }
    
    private void WriteChange(ServerActions.AbstractAction change, bool forLoading = false)
    {
        lock (WaitingChanges)
        {
            if (Loaded || forLoading)
                Socket?.SendJsonAsync(change).GetAwaiter().GetResult();
            else
                WaitingChanges.Enqueue(change);
        }
    }
    
    public void Welcome()
    {
        WriteChange(
            new ServerActions.Welcome(Id),
            true
        );
    }

    public void WritePage(Page page)
    {
        var head = page.Head.RenderedContent.WhereNotNull().Select(ToCode).ToList();
        List<string> beforeScript = [];
        List<string> afterScript = [];
        string? script = null;
        foreach (var part in page.Body.RenderedContent)
            if (part != null)
                if (part is SystemScriptReference s)
                    script = s.Location;
                else if (part is not LoadingScreen)
                    if (script != null)
                        afterScript.Add(ToCode(part));
                    else
                        beforeScript.Add(ToCode(part));
        if (script == null)
            throw new ForcedResponse(StatusResponse.Teapot);
        WriteChange(
            new ServerActions.FullPage(
                head,
                beforeScript,
                afterScript,
                script
            ),
            true
        );
        lock (WaitingChanges)
        {
            while (WaitingChanges.TryDequeue(out var change))
                Socket?.SendJsonAsync(change).GetAwaiter().GetResult();
            Loaded = true;
        }
    }

    private static string ToCode(AbstractMarkdownPart part)
        => string.Join("", part.EnumerateChunks());
    
    public void AttributeChanged(WatchedElement element, string attributeName)
    {
        var path = element.GetPath();
        if (path == null)
            return;
        var attributeValue = element.GetAttribute(attributeName);
        WriteChange(
            new ServerActions.ChangeAttribute(
                path,
                attributeName,
                attributeValue
            )
        );
    }
    
    public void ElementRemoved(WatchedElement child)
    {
        var path = child.GetPath();
        if (path == null)
            return;
        WriteChange(
            new ServerActions.RemoveElement(
                path
            )
        );
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
        WriteChange(
            new ServerActions.AddElementBefore(
                path,
                html
            )
        );
    }
    
    public void ElementAddedAfter(WatchedElement child, WatchedElement predecessor)
    {
        var path = predecessor.GetPath();
        if (path == null)
            return;
        var html = string.Join("", child.EnumerateChunks());
        WriteChange(
            new ServerActions.AddElementAfter(
                path,
                html
            )
        );
    }
    
    public void ContentChanged(WatchedElement element)
    {
        var path = element.GetPath();
        if (path == null)
            return;
        var content = string.Join("", element.RenderedContainers.WhereNotNull().SelectMany(c => c.WhereNotNull().SelectMany(e => e.EnumerateChunks())));
        WriteChange(
            new ServerActions.ChangeContent(
                path,
                content
            )
        );
    }
    
    public void SetValue(WatchedElement element, object value)
    {
        var path = element.GetPath();
        if (path == null)
            return;
        WriteChange(
            new ServerActions.SetValue(
                path,
                value
            )
        );
    }
    
    public void InternalReload()
    {
        WriteChange(
            new ServerActions.InternalReload()
        );
    }
    
    public void Navigate(string location)
    {
        WriteChange(
            new ServerActions.Navigate(
                location
            )
        );
    }
    
    public void BrowserReload()
    {
        WriteChange(
            new ServerActions.BrowserReload()
        );
    }
    
    public void BrowserAlert(string message)
    {
        WriteChange(
            new ServerActions.BrowserAlert(message)
        );
    }
}