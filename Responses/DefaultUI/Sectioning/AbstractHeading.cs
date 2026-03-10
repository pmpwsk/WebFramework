using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI heading.
/// </summary>
public abstract class AbstractHeading : OptionalIdElement
{
    private readonly IconAndTextContainer ContentContainer;
    
    protected AbstractHeading(IconAndText content)
    {
        ContentContainer = new(this, content);
        FixedAttributes.Add(("class", "wf-heading"));
    }
    
    /// <summary>
    /// The heading's content.
    /// </summary>
    public IconAndText Content
    {
        get => ContentContainer.Content;
        set => ContentContainer.Content = value;
    }
}