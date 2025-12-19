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
    /// The overlay behind menus.
    /// </summary>
    private readonly RequiredWatchedContainer<CustomElement> MenuBackgroundContainer;
    
    /// <summary>
    /// The overlay behind dialogs.
    /// </summary>
    private readonly RequiredWatchedContainer<CustomElement> DialogBackgroundContainer;
    
    private readonly RequiredWatchedContainer<NavBar> NavBarContainer;
    
    private readonly RequiredWatchedContainer<PageContent> PageContentContainer;
    
    /// <summary>
    /// The available menus.
    /// </summary>
    public readonly ListWatchedContainer<Menu> Menus;
    
    /// <summary>
    /// The available dialogs.
    /// </summary>
    public readonly ListWatchedContainer<Dialog> Dialogs;
    
    private readonly RequiredWatchedContainer<LoadingScreen> LoadingScreenContainer;
    
    /// <summary>
    /// The default UI script.
    /// </summary>
    private readonly RequiredWatchedContainer<SystemScriptReference> DefaultScript;
    
    /// <summary>
    /// The scripts to load.
    /// </summary>
    public readonly ListWatchedContainer<ScriptReference> Scripts;
    
    public Body(Request req, IEnumerable<Menu>? menus = null, IEnumerable<Dialog>? dialogs = null, IEnumerable<ScriptReference>? scripts = null)
    {
        TopCoverContainer = new(this, new("span") { Class = "wf-top-cover" });
        MenuBackgroundContainer = new(this, new("span") { Class = "wf-menu-background" });
        DialogBackgroundContainer = new(this, new("span") { Class = "wf-dialog-background" });
        NavBarContainer = new(this, new());
        PageContentContainer = new(this, new(req));
        Menus = new(this, menus ?? []);
        Dialogs = new(this, dialogs ?? []);
        LoadingScreenContainer = new(this, new(false));
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
    
    /// <summary>
    /// The page's loading screen.
    /// </summary>
    public LoadingScreen LoadingScreen
    {
        get => LoadingScreenContainer.Element;
        set => LoadingScreenContainer.Element = value;
    }
    
    public override string RenderedTag
        => "body";

    internal override string? FixedSystemId
        => "body";
}