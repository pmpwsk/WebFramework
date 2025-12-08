using System.Text.Json;
using uwap.WebFramework.Responses.DefaultUI;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to watch and distribute changes to default UI elements.
/// </summary>
public class ChangeWatcher
{
    public string Id;
    
    public EventResponse? EventResponse = null;

    private readonly Queue<string> WaitingChanges = [];

    private bool Loaded = false; 
    
    internal ChangeWatcher(string id)
    {
        Id = id;
    }
    
    private void WriteChange(object change, bool forLoading = false)
    {
        var data = JsonSerializer.Serialize(change);
        lock (WaitingChanges)
        {
            if (Loaded || forLoading)
                EventResponse?.EventMessage(data).GetAwaiter().GetResult();
            else
                WaitingChanges.Enqueue(data);
        }
    }

    public void WritePage(Page page)
    {
        WriteChange(new { type = "Head", elements = page.Head.RenderedContent.WhereNotNull().Select(ToCode).ToList() }, true);
        List<string> beforeScript = [];
        List<string> afterScript = [];
        bool passedScript = false;
        foreach (var part in page.Body.RenderedContent)
            if (part != null)
                if (part is SystemScriptReference)
                    passedScript = true;
                else if (passedScript)
                    afterScript.Add(ToCode(part));
                else
                    beforeScript.Add(ToCode(part));
        WriteChange(new { type = "BodyBeforeScript", elements = beforeScript }, true);
        WriteChange(new { type = "BodyAfterScript", elements = afterScript }, true);
        lock (WaitingChanges)
        {
            while (WaitingChanges.TryDequeue(out var data))
                EventResponse?.EventMessage(data).GetAwaiter().GetResult();
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