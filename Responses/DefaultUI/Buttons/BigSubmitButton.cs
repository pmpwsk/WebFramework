using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form.
/// </summary>
public class BigSubmitButton : OptionalIdElement
{
    private readonly OptionalWatchedAttribute OverrideActionAttribute;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    private readonly RequiredWatchedContainer<Heading3> HeaderContainer;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    public readonly ListWatchedContainer<Paragraph> Paragraphs;
    
    public BigSubmitButton(IconAndText text, IEnumerable<string> subtexts, string? overrideAction = null)
    {
        var header = new Heading3(text);
        header.FixedAttributes.Remove(("class", "wf-heading"));
        HeaderContainer = new(this, header);
        Paragraphs = new(this, subtexts.Select(subtext => new Paragraph(subtext)));
        OverrideActionAttribute = new(this, "formaction", overrideAction);
        FixedAttributes.Add(("class", "wf-button wf-button-is-container"));
        FixedAttributes.Add(("draggable", "false"));
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public IconAndText Text
    {
        get => HeaderContainer.Element.Content;
        set => HeaderContainer.Element.Content = value;
    }
    
    /// <summary>
    /// The URL of the action to use instead of the form's action.
    /// </summary>
    public string? OverrideAction
    {
        get => OverrideActionAttribute.Value;
        set => OverrideActionAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "button";
}