using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI HTML head.
/// </summary>
public class Head : WatchedElement
{
    private readonly RequiredWatchedContainer<Title> TitleContainer;
    
    private readonly OptionalWatchedContainer<Metadata> DescriptionContainer;
    
    /// <summary>
    /// The page's viewport configuration.
    /// </summary>
    private readonly RequiredWatchedContainer<Metadata> ViewportSettingsContainer;
    
    /// <summary>
    /// The page's character set configuration.
    /// </summary>
    private readonly RequiredWatchedContainer<Charset> CharsetContainer;
    
    private readonly OptionalWatchedContainer<CanonicalReference> CanonicalContainer;
    
    private readonly OptionalWatchedContainer<Favicon> FaviconContainer;
    
    private readonly RequiredWatchedContainer<StyleReference> LayoutStyleContainer;
    
    private readonly RequiredWatchedContainer<StyleReference> ThemeStyleContainer;
    
    /// <summary>
    /// The CSS file references.
    /// </summary>
    public readonly ListWatchedContainer<StyleReference> Styles;
    
    public Head(Request req, IEnumerable<StyleReference>? styles = null)
    {
        TitleContainer = new(this, new("Untitled", Server.Config.Domains.TitleExtensions.GetValueAny(req.Domains)));
        DescriptionContainer = new(this, null);
        ViewportSettingsContainer = new(this, new("viewport", "width=device-width, initial-scale=1.0, interactive-widget=resizes-content"));
        CharsetContainer = new(this, new());
        var canonicalUrl = req.CanonicalUrl;
        CanonicalContainer = new(this, canonicalUrl == null ? null : new(canonicalUrl));
        FaviconContainer = new(this, null);
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
    /// The page's description.
    /// </summary>
    public string? Description
    {
        get => DescriptionContainer.Element?.Content;
        set
        {
            if (value == null)
                DescriptionContainer.Element = null;
            else if (DescriptionContainer.Element == null)
                DescriptionContainer.Element = new("description", value);
            else
                DescriptionContainer.Element.Content = value;
        }
    }
    
    /// <summary>
    /// The page's icon file.
    /// </summary>
    public Favicon? Favicon
    {
        get => FaviconContainer.Element;
        set => FaviconContainer.Element = value;
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

    internal override string? FixedSystemId
        => "head";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            TitleContainer,
            DescriptionContainer,
            ViewportSettingsContainer,
            CharsetContainer,
            CanonicalContainer,
            FaviconContainer,
            LayoutStyleContainer,
            ThemeStyleContainer,
            Styles
        ];
}