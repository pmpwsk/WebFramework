namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that opens/closes the sidebar.
/// </summary>
public class SidebarButton() : ButtonWithText("Go to")
{
    public override string RenderedTag
        => "button";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("type", "button"),
            ("class", "wf-nav-menu-toggle")
        ];
}