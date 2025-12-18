using Microsoft.AspNetCore.Http;
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
    
    public Page(Request req, bool dynamic) : base(req, dynamic)
    {
        HeadContainer = new(this, new(req));
        BodyContainer = new(this, new(req));
        Presets.ModifyPage(req, this);
        if (dynamic && !req.IsInternal)
        {
            Title = "...";
            Sections.Add(new("Loading"));
            throw new ForcedResponse(this);
        }
    }
    
    /// <summary>
    /// The head element.
    /// </summary>
    public Head Head
        => HeadContainer.Element;
    
    /// <summary>
    /// The body element.
    /// </summary>
    public Body Body
        => BodyContainer.Element;
    
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
    /// The page's description.
    /// </summary>
    public string? Description
    {
        get => HeadContainer.Element.Description;
        set => HeadContainer.Element.Description = value;
    }
    
    /// <summary>
    /// The page's icon file.
    /// </summary>
    public Favicon? Favicon
    {
        get => HeadContainer.Element.Favicon;
        set => HeadContainer.Element.Favicon = value;
    }

    /// <summary>
    /// The CSS file references.
    /// </summary>
    public ListWatchedContainer<StyleReference> Styles
        => HeadContainer.Element.Styles;
    
    /// <summary>
    /// The layout CSS file.
    /// </summary>
    public StyleReference LayoutStyle
    {
        get => HeadContainer.Element.LayoutStyle;
        set => HeadContainer.Element.LayoutStyle = value;
    }
    
    /// <summary>
    /// The theme CSS file.
    /// </summary>
    public StyleReference ThemeStyle
    {
        get => HeadContainer.Element.ThemeStyle;
        set => HeadContainer.Element.ThemeStyle = value;
    }
    
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
    /// The available menus.
    /// </summary>
    public ListWatchedContainer<Dialog> Dialogs
        => BodyContainer.Element.Dialogs;
    
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
    
    /// <summary>
    /// Adds a simple dialog with the given message and a button to close the dialog, assuming the page is dynamic.
    /// </summary>
    public void AddDynamicError(params string[] messages)
    {
        foreach (var other in Dialogs.EnumerateTyped().ToList())
            if (other.Id == "wf-dynamic-error" || other.IsOpen)
                Dialogs.Remove(other);
        
        var dialog = new Dialog
        (
            "wf-dynamic-error",
            "Error",
            true,
            [
                ..messages.Select(message => new Paragraph(message)),
                new PopupButton("Okay", "wf-dynamic-error")
            ]
        );
        Dialogs.Add(dialog);
    }
    
    public override IEnumerable<string> EnumerateChunks()
    {
        yield return $"<!DOCTYPE html><html{(WatchedUrl == null ? "" : $" data-wf-url=\"{WatchedUrl}\"")}>";
        
        foreach (var container in RenderedContainers.WhereNotNull())
            foreach (var element in container.WhereNotNull())
                foreach (var chunk in element.EnumerateChunks())
                    yield return chunk;
        
        yield return "</html>";
    }

    public override Task Respond(Request req, HttpContext context)
    {
        context.Response.ContentType = "text/html;charset=utf-8";
        return base.Respond(req, context);
    }
}