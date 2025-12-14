using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI HTML body.
/// </summary>
public class Body : WatchedElement
{
    /// <summary>
    /// The cover behind the navigation bar.
    /// </summary>
    private readonly RequiredWatchedContainer<CustomElement> TopCoverContainer;
    
    /// <summary>
    /// The overlay behind pop-ups.
    /// </summary>
    private readonly RequiredWatchedContainer<CustomElement> OverlayBackgroundContainer;
    
    private readonly RequiredWatchedContainer<NavBar> NavBarContainer;
    
    private readonly RequiredWatchedContainer<PageContent> PageContentContainer;
    
    /// <summary>
    /// The available menus.
    /// </summary>
    public readonly ListWatchedContainer<Menu> Menus;
    
    /// <summary>
    /// The default UI script.
    /// </summary>
    private readonly RequiredWatchedContainer<SystemScriptReference> DefaultScript;
    
    /// <summary>
    /// The scripts to load.
    /// </summary>
    public readonly ListWatchedContainer<ScriptReference> Scripts;
    
    public Body(Request req, IEnumerable<Menu>? menus = null, IEnumerable<ScriptReference>? scripts = null)
    {
        TopCoverContainer = new(this, new("span") { Class = "wf-top-cover" });
        OverlayBackgroundContainer = new(this, new("span") { Class = "wf-overlay-background" });
        NavBarContainer = new(this, new());
        PageContentContainer = new(this, new(req));
        Menus = new(this, menus ?? []);
        DefaultScript = new(this, new(req));
        Scripts = new(this, scripts ?? []);
    }
    
    /// <summary>
    /// The navigation bar.
    /// </summary>
    public NavBar NavBar
    {
        get => NavBarContainer.Element;
        set => NavBarContainer.Element = value;
    }
    
    /// <summary>
    /// The page's content.
    /// </summary>
    public PageContent PageContent
    {
        get => PageContentContainer.Element;
        set => PageContentContainer.Element = value;
    }
    
    public override string RenderedTag
        => "body";

    internal override string? FixedSystemId
        => "body";
}