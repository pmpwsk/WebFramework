using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that opens/closes a specific menu.
/// </summary>
public class MenuButton : ButtonWithText
{
    private readonly RequiredWatchedAttribute MenuIdAttribute;
    
    public MenuButton(string text, string menuId) : base(text)
    {
        MenuIdAttribute = new(this, "data-wf-target-id", menuId);
        FixedAttributes.Add(("type", "button"));
        FixedAttributes.Add(("class", "wf-menu-toggle"));
    }
    
    /// <summary>
    /// The ID of the menu to control.
    /// </summary>
    public string MenuId
    {
        get => MenuIdAttribute.Value;
        set => MenuIdAttribute.Value = value;
    }

    public override string RenderedTag
        => "button";
}