namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that opens/closes the sidebar.
/// </summary>
public class SidebarButton : ButtonWithText
{
    public SidebarButton() : base("Go to")
    {
        FixedAttributes.Add(("type", "button"));
        FixedAttributes.Add(("class", "wf-nav-menu-toggle"));
    }

    public override string RenderedTag
        => "button";
}