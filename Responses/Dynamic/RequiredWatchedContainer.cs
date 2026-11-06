namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to contain a single UI element.
/// </summary>
public class RequiredWatchedContainer<T> : AbstractWatchedContainer where T : AbstractMarkdownPart
{
    private T _Element;
    
    public RequiredWatchedContainer(IWatchedParent parent, T element) : base(parent)
    {
        _Element = element;
        WelcomeChild(element);
    }

    /// <summary>
    /// The actual element.
    /// </summary>
    public T Element
    {
        get => _Element;
        set => SetElement(value, true);
    }
    
    /// <summary>
    /// Sets the element without applying it to the watcher.
    /// </summary>
    public void SetElementWithoutNotifying(T value)
        => SetElement(value, false);
    
    private void SetElement(T value, bool notify)
    {
        if (HasUnwatchedChildren || value is not WatchedElement)
        {
            AbandonChild(_Element);
            _Element = value;
            WelcomeChild(value);
            if (notify && Parent is WatchedElement watchedParent)
                Parent.ChangeWatcher?.ContentChanged(watchedParent);
        }
        else
        {
            if (notify && _Element is WatchedElement old)
                Parent.ChangeWatcher?.ElementRemoved(old);
            AbandonChild(_Element);
                
            _Element = value;
            WelcomeChild(value);
            if (notify && value is WatchedElement watchedElement)
                Parent.ChangeWatcher?.ElementAdded(watchedElement, Parent, FindElementBefore(), FindElementAfter());
        }
    }
    
    public override IEnumerator<AbstractMarkdownPart?> GetEnumerator()
    {
        yield return Element;
    }
}