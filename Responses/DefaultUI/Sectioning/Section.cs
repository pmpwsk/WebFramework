using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI section.
/// </summary>
public class Section : OptionalIdElement
{
    private readonly RequiredWatchedContainer<Heading1> HeaderContainer;
    
    /// <summary>
    /// The section's subsections.
    /// </summary>
    public readonly ListWatchedContainer<AbstractSubsection> Subsections;
    
    public Section(string heading, IEnumerable<AbstractSubsection>? subsections = null)
    {
        HeaderContainer = new(this, new(heading));
        Subsections = new(this, subsections ?? []);
    }

    /// <summary>
    /// The section's heading element.
    /// </summary>
    public Heading1 Header
    {
        get => HeaderContainer.Element;
        set => HeaderContainer.Element = value;
    }
    
    public override string RenderedTag
        => "section";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            HeaderContainer,
            Subsections
        ];
}