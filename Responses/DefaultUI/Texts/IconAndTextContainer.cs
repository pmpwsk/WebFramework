using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A container for an optional icon and text.
/// </summary>
public class IconAndTextContainer(IWatchedParent parent, IconAndText content)
{
    private IconAndText _Content = content;
    
    private readonly OptionalWatchedContainer<ItalicsText> IconContainer = new(parent, content.Icon == null ? null : new() { Class = content.Icon });
    
    private readonly OptionalWatchedContainer<MarkdownText> TextContainer = new(parent, content.Text == null ? null : new(content.GeneratedText));
    
    public IconAndText Content
    {
        get => _Content;
        set
        {
            if (value.Icon == null)
            {
                if (IconContainer.Element != null)
                    IconContainer.Element = null;
            }
            else
            {
                if (IconContainer.Element == null)
                    IconContainer.Element = new() { Class = value.Icon };
                else if (IconContainer.Element.Class != value.Icon)
                    IconContainer.Element.Class = value.Icon;
            }
            
            if (value.Text == null)
            {
                if (TextContainer.Element != null)
                    TextContainer.Element = null;
            }
            else if (TextContainer.Element == null || TextContainer.Element.Text != value.GeneratedText)
                TextContainer.Element = new(value.GeneratedText);
        }
    }
}