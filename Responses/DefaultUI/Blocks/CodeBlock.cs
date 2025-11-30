using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An text block that is formatted as code.
/// </summary>
public class CodeBlock : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public CodeBlock(IEnumerable<AbstractMarkdownPart>? content = null)
    {
        Content = new(this, content ?? []);
    }
    
    public CodeBlock(params string[] lines) : this(LineBreak.Convert(lines))
    {
    }
    
    public override string RenderedTag
        => "pre";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            Content
        ];
}