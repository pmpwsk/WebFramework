using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A group of default UI navigation buttons.
/// </summary>
public class NavIsland : OptionalIdElement
{
    /// <summary>
    /// The navigation buttons in the group.
    /// </summary>
    public readonly ListWatchedContainer<AbstractButton> Buttons;
    
    public NavIsland(IEnumerable<AbstractButton> buttons)
    {
        Buttons = new(this, buttons);
    }
    
    public override string RenderedTag
        => "div";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            Buttons
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-nav-island")
        ];
}