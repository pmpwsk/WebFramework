using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An inline text part that is formatted as bold.
/// </summary>
public class BoldText : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public BoldText(IEnumerable<AbstractMarkdownPart>? content = null)
    {
        Content = new(this, content ?? []);
    }
    
    public override string RenderedTag
        => "b";
}