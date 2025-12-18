using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI dialog.
/// </summary>
public class Dialog : RequiredIdElement
{
    private readonly RequiredWatchedContainer<Heading2> HeaderContainer;
    
    private readonly OptionalWatchedAttribute IsOpenAttribute;
    
    /// <summary>
    /// The menu's items.
    /// </summary>
    public readonly ListWatchedContainer<AbstractElement> Items;
    
    public Dialog(string id, string heading, bool isOpen, IEnumerable<AbstractElement>? items = null) : base(id)
    {
        HeaderContainer = new(this, new(heading));
        IsOpenAttribute = new(this, "class", isOpen ? "wf-is-open" : null);
        Items = new(this, items ?? []);
        FixedAttributes.Add(("class", "wf-dialog"));
    }
    
    public override string RenderedTag
        => "div";
    
    /// <summary>
    /// The menu's heading element.
    /// </summary>
    public Heading2 Header
    {
        get => HeaderContainer.Element;
        set => HeaderContainer.Element = value;
    }
    
    /// <summary>
    /// Whether the dialog is currently open.
    /// </summary>
    public bool IsOpen
    {
        get => IsOpenAttribute.Value == "wf-is-open";
        set => IsOpenAttribute.Value = value ? "wf-is-open" : null;
    }
}