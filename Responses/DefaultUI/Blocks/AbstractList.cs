using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract list.
/// </summary>
public abstract class AbstractList : OptionalIdElement
{
    /// <summary>
    /// The items in the list.
    /// </summary>
    public readonly ListWatchedContainer<ListItem> Items;
    
    protected AbstractList(IEnumerable<ListItem> items)
    {
        Items = new(this, items);
    }
}