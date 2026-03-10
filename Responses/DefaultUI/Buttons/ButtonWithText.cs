using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI button with a text.
/// </summary>
public abstract class ButtonWithText : AbstractButton
{
    /// <summary>
    /// The object describing the button's text.
    /// </summary>
    private readonly IconAndTextContainer ContentContainer;
    
    protected ButtonWithText(IconAndText content)
    {
        ContentContainer = new(this, content);
        FixedAttributes.Add(("class", "wf-button"));
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public IconAndText Content
    {
        get => ContentContainer.Content;
        set => ContentContainer.Content = value;
    }
}