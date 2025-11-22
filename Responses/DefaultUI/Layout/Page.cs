using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An HTML page.
/// </summary>
public class Page : AbstractWatchablePage
{
    /// <summary>
    /// The page's HeadContainer.Element.
    /// </summary>
    private readonly RequiredWatchedContainer<Head> HeadContainer;
    
    /// <summary>
    /// The page's BodyContainer.Element.
    /// </summary>
    private readonly RequiredWatchedContainer<Body> BodyContainer;
    
    public Page(Request req, bool dynamic) : base(dynamic)
    {
        HeadContainer = new(this, new(req));
        BodyContainer = new(this, new(req));
        Presets.ModifyPage(req, this);
    }
    
    /// <summary>
    /// The page's title.
    /// </summary>
    public string Title
    {
        get => HeadContainer.Element.Title.Text;
        set => HeadContainer.Element.Title.Text = value;
    }
    
    /// <summary>
    /// The page's title suffix.
    /// </summary>
    public string? TitleSuffix
    {
        get => HeadContainer.Element.Title.Suffix;
        set => HeadContainer.Element.Title.Suffix = value;
    }

    /// <summary>
    /// The CSS file references.
    /// </summary>
    public ListWatchedContainer<StyleReference> Styles
        => HeadContainer.Element.Styles;
    
    /// <summary>
    /// The navigation bar.
    /// </summary>
    public NavBar NavBar
    {
        get => BodyContainer.Element.NavBar;
        set => BodyContainer.Element.NavBar = value;
    }
    
    /// <summary>
    /// The available menus.
    /// </summary>
    public ListWatchedContainer<Menu> Menus
        => BodyContainer.Element.Menus;
    
    /// <summary>
    /// The JavaScript file references.
    /// </summary>
    public ListWatchedContainer<ScriptReference> Scripts
        => BodyContainer.Element.Scripts;
    
    /// <summary>
    /// The sidebar for specific navigation.
    /// </summary>
    public Sidebar Sidebar
    {
        get => BodyContainer.Element.PageContent.Sidebar;
        set => BodyContainer.Element.PageContent.Sidebar = value;
    }
    
    /// <summary>
    /// The page's sections.
    /// </summary>
    public ListWatchedContainer<Section> Sections
        => BodyContainer.Element.PageContent.Main.Sections;
    
    /// <summary>
    /// The page's footer.
    /// </summary>
    public Footer? Footer
    {
        get => BodyContainer.Element.PageContent.Main.Footer;
        set => BodyContainer.Element.PageContent.Main.Footer = value;
    }
    
    public override IEnumerable<string> EnumerateChunks()
    {
        yield return $"<!DOCTYPE html><html{(ChangeWatcher == null ? "" : $" data-wf-watcher=\"{ChangeWatcher.Id}\"")}>";
        
        foreach (var container in RenderedContainers.WhereNotNull())
            foreach (var element in container.WhereNotNull())
                foreach (var chunk in element.EnumerateChunks())
                    yield return chunk;
        
        yield return "</html>";
    }

    public override Task Respond(Request req)
    {
        req.Context.Response.ContentType = "text/html;charset=utf-8";
        return base.Respond(req);
    }

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            HeadContainer,
            BodyContainer
        ];
}