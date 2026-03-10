using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button with an integrated form that executes an action on the server.
/// </summary>
public class ServerActionButton : AbstractButton, IActionHaver
{
    public ActionHandler Action { get; set; }
    
    private readonly RequiredWatchedContainer<SubmitButton> SubmitContainer;
    
    public ServerActionButton(IconAndText content, ActionHandler action)
    {
        SubmitContainer = new(this, new(content));
        Action = action;
        FixedAttributes.Add(("class", "wf-server-form"));
        FixedAttributes.Add(("method", "post"));
        FixedAttributes.Add(("enctype", "multipart/form-data"));
        FixedAttributes.Add(("action", "#"));
    }
    
    /// <summary>
    /// The actual submit button.
    /// </summary>
    public SubmitButton Submit
    {
        get => SubmitContainer.Element;
        set => SubmitContainer.Element = value;
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public IconAndText Content
    {
        get => Submit.Content;
        set => Submit.Content = value;
    }
    
    public override string RenderedTag
        => "form";
}