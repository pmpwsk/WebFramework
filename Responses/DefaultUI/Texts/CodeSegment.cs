using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An inline text part that is formatted as code.
/// </summary>
public class CodeSegment : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public CodeSegment(IEnumerable<AbstractMarkdownPart>? content = null)
    {
        Content = new(this, content ?? []);
    }
    
    public override string RenderedTag
        => "code";
}