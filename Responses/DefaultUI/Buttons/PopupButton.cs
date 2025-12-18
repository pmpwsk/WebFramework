using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that opens/closes a specific menu or dialog.
/// </summary>
public class PopupButton : ButtonWithText
{
    private readonly RequiredWatchedAttribute PopupIdAttribute;
    
    public PopupButton(string text, string popupId) : base(text)
    {
        PopupIdAttribute = new(this, "data-wf-target-id", popupId);
        FixedAttributes.Add(("type", "button"));
        FixedAttributes.Add(("class", "wf-popup-toggle"));
    }
    
    /// <summary>
    /// The ID of the menu or dialog to control.
    /// </summary>
    public string PopupId
    {
        get => PopupIdAttribute.Value;
        set => PopupIdAttribute.Value = value;
    }

    public override string RenderedTag
        => "button";
}