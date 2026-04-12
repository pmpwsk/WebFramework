using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
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
    
    /// <summary>
    /// The stack of states for the dynamic dialog.
    /// </summary>
    private readonly Stack<(IconAndText Heading, List<AbstractElement> Items, ActionHandler Action)> DynamicDialogStack = [];
    
    public Page(Request req, bool dynamic, string? title) : base(req, dynamic)
    {
        HeadContainer = new(this, new(req, title));
        BodyContainer = new(this, new(req, dynamic));
        Presets.ModifyPage(req, this);
        if (dynamic && !req.IsInternal)
        {
            Title = "...";
            Body.LoadingScreen.IsOpen = true;
            throw new ForcedResponse(this);
        }
    }
    
    public Page(Request req, bool dynamic, string? title, IEnumerable<Section> sections) : this(req, dynamic, title)
    {
        foreach (var section in sections)
            Sections.Add(section);
    }
    
    public Page(Request req, bool dynamic, string? title, IEnumerable<AbstractElement> sidebar, IEnumerable<Section> sections) : this(req, dynamic, title, sections)
    {
        foreach (var element in sidebar)
            Sidebar.Items.Add(element);
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
    /// The page's color scheme.
    /// </summary>
    public ColorSchemeOption ColorScheme
    {
        get => HeadContainer.Element.ColorScheme;
        set => HeadContainer.Element.ColorScheme = value;
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
    /// The page's dynamic dialog.
    /// </summary>
    public ServerFormDialog? DynamicDialog
        => BodyContainer.Element.DynamicDialog;
    
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
    /// Applies the given parameters to the dynamic dialog and ensures that it's open.
    /// </summary>
    private void SetDynamicDialogParameters(IconAndText heading, List<AbstractElement> elements, ActionHandler action)
    {
        if (DynamicDialog == null)
            throw new Exception("There is no dynamic dialog.");
        
        DynamicDialog.Action = action;
        DynamicDialog.Header.Content = heading;
        DynamicDialog.Items.ReplaceAll(elements);
        
        DynamicDialog.IgnoreActions = false;
        if (!DynamicDialog.IsOpen)
            DynamicDialog.IsOpen = true;
    }
    
    /// <summary>
    /// Closes the current dynamic popup if one is present.
    /// </summary>
    public void CloseDynamicDialog()
    {
        if (DynamicDialog != null && DynamicDialog.IsOpen)
        {
            DynamicDialogStack.Clear();
            
            DynamicDialog.Action = Nothing.EmptyHandler;
            DynamicDialog.IgnoreActions = true;
            DynamicDialog.IsOpen = false;
        }
    }
    
    /// <summary>
    /// Returns the dynamic dialog to its previous state or closes it if no previous state is present.
    /// </summary>
    public void ReturnDynamicDialog()
    {
        if (DynamicDialog == null)
            throw new Exception("There is no dynamic dialog.");
        
        if (DynamicDialogStack.TryPop(out var parameters))
            SetDynamicDialogParameters(parameters.Heading, parameters.Items, parameters.Action);
        else
            CloseDynamicDialog();
    }
    
    /// <summary>
    /// Opens a dialog with the given elements, assuming the page is dynamic.
    /// </summary>
    public void OpenDynamicDialog(IconAndText heading, List<AbstractElement> elements, ActionHandler action)
    {
        if (DynamicDialog == null)
            throw new Exception("There is no dynamic dialog.");
        
        if (DynamicDialog.IsOpen)
            DynamicDialogStack.Push((DynamicDialog.Header.Content, DynamicDialog.Items.EnumerateTyped().ToList(), DynamicDialog.Action));
        
        SetDynamicDialogParameters(heading, elements, action);
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