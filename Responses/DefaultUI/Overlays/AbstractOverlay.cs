using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI overlay.
/// </summary>
public abstract class AbstractOverlay : RequiredIdElement
{
    private readonly RequiredWatchedContainer<Heading2> HeaderContainer;
    
    /// <summary>
    /// The overlay's items.
    /// </summary>
    public readonly ListWatchedContainer<AbstractElement> Items;
    
    public bool IgnoreActions = false;
    
    protected AbstractOverlay(string id, IconAndText heading, IEnumerable<AbstractElement>? items) : base(id)
    {
        HeaderContainer = new(this, new(heading));
        Items = new(this, items ?? []);
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
}