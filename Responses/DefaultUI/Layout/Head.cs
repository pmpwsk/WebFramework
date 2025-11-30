using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI HTML head.
/// </summary>
public class Head : WatchedElement
{
    private readonly RequiredWatchedContainer<Title> TitleContainer;
    
    /// <summary>
    /// The page's viewport configuration.
    /// </summary>
    private readonly RequiredWatchedContainer<Metadata> ViewportSettingsContainer;
    
    /// <summary>
    /// The default UI style.
    /// </summary>
    private readonly RequiredWatchedContainer<StyleReference> DefaultStyle;
    
    /// <summary>
    /// The CSS file references.
    /// </summary>
    public readonly ListWatchedContainer<StyleReference> Styles;
    
    public Head(Request req, List<StyleReference>? styles = null)
    {
        TitleContainer = new(this, new("Untitled", Server.Config.Domains.TitleExtensions.GetValueAny(req.Domains)));
        ViewportSettingsContainer = new(this, new("viewport", "width=device-width, initial-scale=1.0, interactive-widget=resizes-content"));
        LayoutStyleContainer = new(this, new(req, $"{Server.Layers.SystemFilesLayerPrefix}/default-ui-layout.css"));
        ThemeStyleContainer = new(this, new(req, $"{Server.Layers.SystemFilesLayerPrefix}/default-ui-theme.css"));
        Styles = new(this, styles ?? []);
    }
    
    /// <summary>
    /// The page's title.
    /// </summary>
    public Title Title
    {
        get => TitleContainer.Element;
        set => TitleContainer.Element = value;
    }
    
    /// <summary>
    /// The layout CSS file.
    /// </summary>
    public StyleReference LayoutStyle
    {
        get => LayoutStyleContainer.Element;
        set => LayoutStyleContainer.Element = value;
    }
    
    /// <summary>
    /// The theme CSS file.
    /// </summary>
    public StyleReference ThemeStyle
    {
        get => ThemeStyleContainer.Element;
        set => ThemeStyleContainer.Element = value;
    }
    
    public override string RenderedTag
        => "head";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            TitleContainer,
            ViewportSettingsContainer,
            LayoutStyleContainer,
            ThemeStyleContainer,
            Styles
        ];
}