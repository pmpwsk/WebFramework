using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A container for an optional icon and text.
/// </summary>
public class IconAndTextContainer(IWatchedParent parent, IconAndText content)
{
    private IconAndText _Content = content;
    
    private readonly OptionalWatchedContainer<Icon> IconContainer
        = new(parent, content.Icon == null ? null : new(content.Icon));
    
    private readonly OptionalWatchedContainer<Text> TextContainer
        = new(parent, content.Text == null ? null : new(content.Text));
    
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
                    IconContainer.Element = new(value.Icon);
                else if (IconContainer.Element.Class != value.Icon)
                    IconContainer.Element.Class = value.Icon;
            }
            
            if (value.Text == null)
            {
                if (TextContainer.Element != null)
                    TextContainer.Element = null;
            }
            else
            {
                if (TextContainer.Element == null)
                    TextContainer.Element = new(value.Text);
                else if (TextContainer.Element.Content.Element.Text != value.Text)
                    TextContainer.Element.Content.Element = new(value.Text);
            }
            
            _Content = value;
        }
    }
}