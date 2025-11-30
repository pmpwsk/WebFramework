using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The default UI navigation bar.
/// </summary>
public class NavBar : WatchedElement
{
    private readonly RequiredWatchedContainer<SidebarButton> SidebarButtonContainer;
    
    /// <summary>
    /// The navigation islands.
    /// </summary>
    public readonly ListWatchedContainer<NavIsland> Islands;
    
    public NavBar(IEnumerable<NavIsland>? navIslands = null)
    {
        SidebarButtonContainer = new(this, new());
        Islands = new(this, navIslands ?? []);
    }
    
    public override string RenderedTag
        => "nav";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            SidebarButtonContainer,
            Islands
        ];
}