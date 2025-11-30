using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The default UI sidebar.
/// </summary>
public class Sidebar : OptionalIdElement
{
    private readonly RequiredWatchedContainer<Heading2> HeaderContainer;
    
    /// <summary>
    /// The menu's items.
    /// </summary>
    public readonly ListWatchedContainer<AbstractButton> Items;
    
    public Sidebar(IEnumerable<AbstractButton>? items = null)
    {
        HeaderContainer = new(this, new("Navigation"));
        Items = new(this, items ?? []);
    }
    
    public override string RenderedTag
        => "aside";
    
    /// <summary>
    /// The sidebar's heading element.
    /// </summary>
    private Heading2 Header
    {
        get => HeaderContainer.Element;
        set => HeaderContainer.Element = value;
    }

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            HeaderContainer,
            Items
        ];
}