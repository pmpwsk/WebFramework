using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI menu.
/// </summary>
public class Menu : RequiredIdElement
{
    private readonly RequiredWatchedContainer<Heading2> HeaderContainer;
    
    /// <summary>
    /// The menu's items.
    /// </summary>
    public readonly ListWatchedContainer<AbstractButton> Items;
    
    public Menu(string id, string heading, IEnumerable<AbstractButton>? items = null) : base(id)
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

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            HeaderContainer,
            Items
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-menu")
        ];
}