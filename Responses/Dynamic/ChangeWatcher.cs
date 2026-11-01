using uwap.WebFramework.Responses.DefaultUI;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to watch and distribute changes to default UI elements.
/// </summary>
public class ChangeWatcher(Request req, string url, SocketResponse socket) : IDisposable
{
    internal readonly Request Request = req;
    
    internal readonly string Url = url;
    
    internal AbstractWatchablePage? Page = null;
    
    internal SocketResponse Socket = socket;
    
    internal async Task LoadAsync()
    {
        var otherResponse = await Server.GetOtherResponseAsync(Request, Url);
        if (otherResponse is not AbstractWatchablePage page)
        {
            ServerActions.AbstractAction action = otherResponse is RedirectResponse redirectResponse
                ? new ServerActions.Navigate(redirectResponse.Location)
                : new ServerActions.BrowserReload();
                        
            otherResponse.Dispose();
            await Socket.SendJsonAsync(
                action
            );
            return;
        }
        
        Page = page;
        Request.Cookies.UseWatcherWriting(this);
        await WritePageAsync();
        Page.ChangeWatcher = this;
    }
    
    internal void Unload()
    {
        if (Page != null)
        {
            Page.ChangeWatcher = null;
            Page.Dispose();
            Page = null;
        }
    }
    
    public async Task ReloadAsync()
    {
        Unload();
        await LoadAsync();
    }
    
    private Task WriteChangeAsync(ServerActions.AbstractAction change)
        => Socket.SendJsonAsync(change);
    
    private void WriteChange(ServerActions.AbstractAction change)
        => WriteChangeAsync(change).GetAwaiter().GetResult();

    public Task WritePageAsync()
    {
        if (Page == null)
            throw new Exception("No page is present.");
        
        var head = Page.Head.RenderedContent.WhereNotNull().Select(ToCode).ToList();
        List<string> beforeScript = [];
        List<string> afterScript = [];
        string? script = null;
        foreach (var part in Page.Body.RenderedContent)
            if (part != null)
                if (part is SystemScriptReference s)
                    script = s.Location;
                else if (part is not LoadingScreen)
                    if (script != null)
                        afterScript.Add(ToCode(part));
                    else
                        beforeScript.Add(ToCode(part));
        if (script == null)
            throw new Exception("Page was generated without a default script.");
        return WriteChangeAsync(
            new ServerActions.FullPage(
                head,
                beforeScript,
                afterScript,
                script
            )
        );
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
    
    public Task SetValueAsync(WatchedElement element, object value)
    {
        var path = element.GetPath();
        if (path == null)
            return Task.CompletedTask;
        return WriteChangeAsync(
            new ServerActions.SetValue(
                path,
                value
            )
        );
    }
    
    public Task NavigateAsync(string location)
        => WriteChangeAsync(new ServerActions.Navigate(location));
    
    public Task BrowserReloadAsync()
        => WriteChangeAsync(new ServerActions.BrowserReload());
    
    public Task BrowserAlertAsync(string message)
        => WriteChangeAsync(new ServerActions.BrowserAlert(message));
    
    public Task FinishActionAsync()
        => WriteChangeAsync(new ServerActions.FinishAction());
    
    public Task CookieActionAsync(string id)
        => WriteChangeAsync(new ServerActions.CookieAction(id));
    
    public void FocusElement(WatchedElement child)
    {
        var path = child.GetPath();
        if (path == null)
            return;
        WriteChange(
            new ServerActions.FocusElement(
                path
            )
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Unload();
    }
}