namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to contain a single UI element.
/// </summary>
public class OptionalWatchedContainer<T> : AbstractWatchedContainer where T : AbstractMarkdownPart
{
    private T? _Element;
    
    public OptionalWatchedContainer(IWatchedParent parent, T? element) : base(parent)
    {
        _Element = element;
        WelcomeChild(element);
    }

    /// <summary>
    /// The actual element.
    /// </summary>
    public T? Element
    {
        get => _Element;
        set
        {
            if (HasUnwatchedChildren || (value != null && value is not WatchedElement))
            {
                AbandonChild(_Element);
                _Element = value;
                WelcomeChild(value);
                if (Parent is WatchedElement watchedParent)
                    Parent.ChangeWatcher?.ContentChanged(watchedParent);
            }
            else
            {
                if (_Element is WatchedElement old)
                    Parent.ChangeWatcher?.ElementRemoved(old);
                AbandonChild(_Element);
                
                _Element = value;
                WelcomeChild(value);
                if (value is WatchedElement watchedElement)
                    Parent.ChangeWatcher?.ElementAdded(watchedElement, Parent, FindElementBefore(), FindElementAfter());
            }
        }
    }
    
    public override IEnumerator<AbstractMarkdownPart?> GetEnumerator()
    {
        yield return Element;
    }
}