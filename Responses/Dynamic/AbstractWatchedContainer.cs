using System.Collections;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A generic class to contain dynamic default UI elements.
/// </summary>
public abstract class AbstractWatchedContainer : IEnumerable<AbstractMarkdownPart?>
{
    /// <summary>
    /// The parent element.
    /// </summary>
    internal readonly IWatchedParent Parent;

    /// <summary>
    /// A generic class to contain dynamic default UI elements.
    /// </summary>
    protected AbstractWatchedContainer(IWatchedParent parent)
    {
        Parent = parent;
        Parent.RenderedContainers.Add(this);
    }

    public void WelcomeChild(AbstractMarkdownPart? part)
    {
        if (part is WatchedElement element)
        {
            element.ParentContainer = this;
            element.SystemId = Parsers.RandomString(12, id => Parent.RenderedContainers.All(container => container == null || container.All(p => p is not WatchedElement e || e.SystemId != id)));
        }
    }
    
    public void AbandonChild(AbstractMarkdownPart? part)
    {
        if (part is WatchedElement element)
        {
            element.ParentContainer = this;
            element.SystemId = null;
        }
    }
    
    public bool HasUnwatchedChildren
        => Parent.RenderedContainers.Any(container => container != null && container.Any(child => child != null && child is not WatchedElement));
    
    public WatchedElement? FindElementBefore()
    {
        bool thisPassed = false;
        foreach (var container in Parent.RenderedContainers.AsEnumerable().Reverse().WhereNotNull())
        {
            if (thisPassed)
                foreach (var element in container.Reverse().WhereNotNull())
                    if (element is WatchedElement child)
                        return child;
            
            if (container == this)
                thisPassed = true;
        }
        
        return null;
    }
    
    public WatchedElement? FindElementAfter()
    {
        bool thisPassed = false;
        foreach (var container in Parent.RenderedContainers.WhereNotNull())
        {
            if (thisPassed)
                foreach (var element in container.WhereNotNull())
                    if (element is WatchedElement child)
                        return child;
            
            if (container == this)
                thisPassed = true;
        }
        
        return null;
    }

    public abstract IEnumerator<AbstractMarkdownPart?> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}