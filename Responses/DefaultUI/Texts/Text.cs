using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An inline text part that is part of a mixed text.
/// </summary>
public class Text : OptionalIdElement
{
    public readonly RequiredWatchedContainer<MarkdownText> Content;
    
    public Text(string content)
    {
        Content = new(this, new(content));
        FixedAttributes.Add(("class", "wf-text"));
    }
    
    public override string RenderedTag
        => "span";
}