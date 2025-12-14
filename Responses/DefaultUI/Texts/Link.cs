using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A link that navigates to a URL.
/// </summary>
public class Link : OptionalIdElement
{
    private readonly RequiredWatchedContainer<MarkdownText> ContentContainer;
    
    private readonly RequiredWatchedAttribute TargetAttribute;
    
    public Link(string text, string target)
    {
        ContentContainer = new(this, new(text));
        TargetAttribute = new(this, "href", target);
    }
    
    /// <summary>
    /// The link's text.
    /// </summary>
    public string Text
    {
        get => ContentContainer.Element.Text;
        set => ContentContainer.Element = new(value);
    }
    
    /// <summary>
    /// The URL to navigate to.
    /// </summary>
    public string Target
    {
        get => TargetAttribute.Value;
        set => TargetAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "a";
}