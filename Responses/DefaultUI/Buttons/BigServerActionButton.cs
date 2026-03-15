using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button with an integrated form that executes an action on the server.
/// </summary>
public class BigServerActionButton : OptionalIdElement, IActionHaver
{
    public ActionHandler Action { get; set; }
    
    private readonly RequiredWatchedContainer<BigSubmitButton> SubmitContainer;
    
    public BigServerActionButton(IconAndText text, IEnumerable<string> subtexts, ActionHandler action)
    {
        SubmitContainer = new(this, new(text, subtexts));
        Action = action;
        FixedAttributes.Add(("class", "wf-server-form"));
        FixedAttributes.Add(("method", "post"));
        FixedAttributes.Add(("enctype", "multipart/form-data"));
        FixedAttributes.Add(("action", "#"));
    }
    
    /// <summary>
    /// The actual submit button.
    /// </summary>
    public BigSubmitButton Submit
    {
        get => SubmitContainer.Element;
        set => SubmitContainer.Element = value;
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public IconAndText Text
    {
        get => Submit.Text;
        set => Submit.Text = value;
    }
    
    public ListWatchedContainer<Paragraph> Paragraphs
        => Submit.Paragraphs;
    
    public override string RenderedTag
        => "form";
}