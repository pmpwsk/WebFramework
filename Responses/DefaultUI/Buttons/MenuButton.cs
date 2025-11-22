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

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            MenuIdAttribute
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("type", "button"),
            ("class", "wf-menu-toggle")
        ];
}