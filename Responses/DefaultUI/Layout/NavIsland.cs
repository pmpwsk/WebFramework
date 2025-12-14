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
        FixedAttributes.Add(("class", "wf-nav-island"));
    }
    
    public override string RenderedTag
        => "div";
}