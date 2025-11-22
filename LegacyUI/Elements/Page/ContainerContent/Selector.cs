namespace uwap.WebFramework.Elements;

/// <summary>
/// Dropdown/popup selector for a container.
/// </summary>
public class Selector : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "select";

    //documentation inherited from IElement
    protected override string? ElementProperties => OnChange == null ? null : $"onchange=\"{OnChange.HtmlValueSafe()}\"";

    /// <summary>
    /// JavaScript command that should be executed when the value changes.
    /// </summary>
    public string? OnChange = null;

    /// <summary>
    /// The possible items for the selector.
    /// </summary>
    public List<SelectorItem> Items;

    /// <summary>
    /// Creates a new dropdown/popup selector for a container with the given items.
    /// </summary>
    public Selector(string id, params string[] items)
    {
        Id = id;
        Items = items.Select(x => new SelectorItem(x)).ToList();
    }

    /// <summary>
    /// Creates a new dropdown/popup selector for a container with the given items.
    /// </summary>
    public Selector(string id, string selectedItem, params string[] items)
    {
        Id = id;
        Items = items.Select(x => new SelectorItem(x, x == selectedItem)).ToList();
    }

    /// <summary>
    /// Creates a new dropdown/popup selector for a container with the given items.
    /// </summary>
    public Selector(string id, IEnumerable<string> items, string? selectedItem = null)
    {
        Id = id;
        Items = items.Select(x => new SelectorItem(x, x == selectedItem)).ToList();
    }

    /// <summary>
    /// Creates a new dropdown/popup selector for a container with the given items.
    /// </summary>
    public Selector(string id, params SelectorItem[] items)
    {
        Id = id;
        Items = [.. items];
    }

    /// <summary>
    /// Creates a new dropdown/popup selector for a container with the given items.
    /// </summary>
    public Selector(string id, IEnumerable<SelectorItem> items)
    {
        Id = id;
        Items = items.ToList();
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener;
        foreach (var item in Items)
            yield return "\t" + item.Export(Unsafe);
        yield return Closer;
    }
}

/// <summary>
/// A possible item for a selector.
/// </summary>
public struct SelectorItem
{
    /// <summary>
    /// The text that is shown for the item.
    /// </summary>
    public string Text;

    /// <summary>
    /// The value of the item.
    /// </summary>
    public string Value;

    /// <summary>
    /// Whether this item is selected (only one item should be selected).
    /// </summary>
    public bool Selected;

    /// <summary>
    /// Creates a new selector item with the given text and the given value.
    /// </summary>
    public SelectorItem(string text, string value, bool selected = false)
    {
        Text = text;
        Value = value;
        Selected = selected;
    }

    /// <summary>
    /// Creates a new selector item with the given text/value (identical).
    /// </summary>
    public SelectorItem(string textAndValue, bool selected = false)
    {
        Text = textAndValue;
        Value = textAndValue;
        Selected = selected;
    }

    /// <summary>
    /// The item as HTML code.
    /// </summary>
    public readonly string Export(bool parentUnsafe)
        => $"<option{(Selected ? " selected" : "")} value=\"{Value.HtmlValueSafe()}\">{Text.HtmlSafe(parentUnsafe)}</option>";
}