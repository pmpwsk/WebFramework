using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form while overriding the action with an action executed on the server.
/// </summary>
public class BigServerSubmitButton : OptionalIdElement, IActionHaver
{
    public ActionHandler Action { get; set; }
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    private readonly RequiredWatchedContainer<Heading3> HeaderContainer;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    public readonly ListWatchedContainer<Paragraph> Paragraphs;
    
    public BigServerSubmitButton(IconAndText text, IEnumerable<string> subtexts, ActionHandler action)
    {
        Action = action;
        var header = new Heading3(text);
        header.FixedAttributes.Remove(("class", "wf-heading"));
        HeaderContainer = new(this, header);
        Paragraphs = new(this, subtexts.Select(subtext => new Paragraph(subtext)));
        FixedAttributes.Add(("class", "wf-button wf-button-is-container wf-server-form-override"));
        FixedAttributes.Add(("type", "submit"));
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
    
    public override string RenderedTag
        => "button";
}