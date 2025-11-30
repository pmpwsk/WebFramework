using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An inline text part that is formatted as italics.
/// </summary>
public class ItalicsText : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public ItalicsText(IEnumerable<AbstractMarkdownPart>? content = null)
    {
        Content = new(this, content ?? []);
    }
    
    public override string RenderedTag
        => "i";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            Content
        ];
}