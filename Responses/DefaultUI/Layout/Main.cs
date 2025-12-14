using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI page's main content.
/// </summary>
public class Main : WatchedElement
{
    /// <summary>
    /// The page's sections.
    /// </summary>
    public readonly ListWatchedContainer<Section> Sections;
    
    private readonly OptionalWatchedContainer<Footer> FooterContainer;
    
    public Main(Request req, IEnumerable<Section>? sections = null)
    {
        Sections = new(this, sections ?? []);
        FooterContainer = new(this, new(Server.Config.Domains.CopyrightNames.TryGetValueAny(out var copyright, req.Domains) ? copyright : req.Domain));
    }
    
    /// <summary>
    /// The page's footer.
    /// </summary>
    public Footer? Footer
    {
        get => FooterContainer.Element;
        set => FooterContainer.Element = value;
    }
    public override string RenderedTag
        => "main";
}