namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to contain multiple UI elements.
/// </summary>
public class ListWatchedContainer<T> : AbstractWatchedContainer, IListWatchedContainer<T> where T : AbstractMarkdownPart?
{
    /// <summary>
    /// The actual elements.
    /// </summary>
    private List<T> _Elements;
    
    public ListWatchedContainer(IWatchedParent parent, IEnumerable<T> elements) : base(parent)
    {
        _Elements = elements.ToList();
        foreach (var element in _Elements)
            WelcomeChild(element);
    }
    
    public void Add(T element)
    {
        var predecessor = _Elements.LastOrDefault();
        
        if (HasUnwatchedChildren || element is not WatchedElement)
        {
            _Elements.Add(element);
            WelcomeChild(element);
            if (Parent is WatchedElement watchedParent)
                Parent.ChangeWatcher?.ContentChanged(watchedParent);
        }
        else
        {
            _Elements.Add(element);
            WelcomeChild(element);
            if (element is WatchedElement watchedElement)
                Parent.ChangeWatcher?.ElementAdded(watchedElement, Parent, predecessor is WatchedElement watchedPredecessor ? watchedPredecessor : FindElementBefore(), FindElementAfter());
        }
    }
    
    public void Remove(T element)
    {
        if (!_Elements.Contains(element))
            return;
        
        if (HasUnwatchedChildren)
        {
            _Elements.Remove(element);
            AbandonChild(element);
            
            if (Parent is WatchedElement watchedParent)
                Parent.ChangeWatcher?.ContentChanged(watchedParent);
        }
        else
        {
            if (element is WatchedElement watchedElement)
                Parent.ChangeWatcher?.ElementRemoved(watchedElement);
            
            _Elements.Remove(element);
            AbandonChild(element);
        }
    }
    
    public void Clear()
    {
        if (HasUnwatchedChildren)
        {
            foreach (var element in _Elements)
                AbandonChild(element);
            _Elements.Clear();
            
            if (Parent is WatchedElement watchedParent)
                Parent.ChangeWatcher?.ContentChanged(watchedParent);
        }
        else
        {
            while (_Elements.FirstOrDefault() is { } first)
                Remove(first);
        }
    }
    
    /// <summary>
    /// Replaces the current elements in the list with the given elements.
    /// </summary>
    public void ReplaceAll(List<T> elements)
    {
        if (HasUnwatchedChildren || elements.Any(element => element is not WatchedElement))
        {
            foreach (var element in _Elements)
                AbandonChild(element);
            _Elements.Clear();
            
            _Elements.AddRange(elements);
            foreach (var element in _Elements)
                WelcomeChild(element);
            
            if (Parent is WatchedElement watchedParent)
                Parent.ChangeWatcher?.ContentChanged(watchedParent);
        }
        else
        {
            while (_Elements.FirstOrDefault() is { } first)
                Remove(first);
            
            foreach (var element in elements)
                Add(element);
        }
    }
    
    /// <summary>
    /// Enumerates the typed elements.
    /// </summary>
    public IEnumerable<T> EnumerateTyped()
    {
        foreach (var element in _Elements)
            yield return element;
    }
    
    public override IEnumerator<AbstractMarkdownPart?> GetEnumerator()
    {
        foreach (var element in _Elements)
            yield return element;
    }
}