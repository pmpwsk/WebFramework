using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The default UI page content.
/// </summary>
public class PageContent : WatchedElement
{
    private readonly RequiredWatchedContainer<Sidebar> SidebarContainer;
    
    private readonly RequiredWatchedContainer<Main> MainContainer;
    
    public PageContent(Request req)
    {
        SidebarContainer = new(this, new());
        MainContainer = new(this, new(req));
    }
    
    /// <summary>
    /// The sidebar for specific navigation.
    /// </summary>
    public Sidebar Sidebar
    {
        get => SidebarContainer.Element;
        set => SidebarContainer.Element = value;
    }
    
    /// <summary>
    /// The page's main content.
    /// </summary>
    public Main Main
    {
        get => MainContainer.Element;
        set => MainContainer.Element = value;
    }
    
    public override string RenderedTag
        => "div";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            SidebarContainer,
            MainContainer
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-content")
        ];
}