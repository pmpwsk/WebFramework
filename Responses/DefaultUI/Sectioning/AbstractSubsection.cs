using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI subsection.
/// </summary>
public abstract class AbstractSubsection : OptionalIdElement
{
    private readonly OptionalWatchedContainer<Heading2> HeaderContainer;
    
    /// <summary>
    /// The subsection's content.
    /// </summary>
    public readonly ListWatchedContainer<AbstractElement> Content;
    
    protected AbstractSubsection(string? heading, List<AbstractElement>? content = null)
    {
        HeaderContainer = new(this, heading == null ? null : new(heading));
        Content = new(this, content ?? []);
    }
    
    /// <summary>
    /// The subsection's heading element.
    /// </summary>
    public Heading2? Header
    {
        get => HeaderContainer.Element;
        set => HeaderContainer.Element = value;
    }

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            HeaderContainer,
            Content
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-subsection")
        ];
}