using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI heading.
/// </summary>
public abstract class AbstractHeading : OptionalIdElement
{
    private readonly RequiredWatchedContainer<MarkdownText> ContentContainer;
    
    protected AbstractHeading(string text)
    {
        ContentContainer = new(this, new(text));
    }
    
    /// <summary>
    /// The heading's text.
    /// </summary>
    public string Text
    {
        get => ContentContainer.Element.Text;
        set => ContentContainer.Element = new(value);
    }

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            ContentContainer
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-heading")
        ];
}