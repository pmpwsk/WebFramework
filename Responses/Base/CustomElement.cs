using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// A custom HTML element with common properties and an optional ID.
/// </summary>
public class CustomElement : OptionalIdElement
{
    /// <summary>
    /// The element's tag name.
    /// </summary>
    public string Tag;
    
    /// <summary>
    /// The element's content/children.
    /// </summary>
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public CustomElement(string tag, List<AbstractMarkdownPart>? content = null)
    {
        Tag = tag;
        Content = new(this, content ?? []);
    }

    public override string RenderedTag => Tag;
}