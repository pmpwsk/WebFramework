using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button with an integrated form that executes an action on the server.
/// </summary>
public class ServerActionButton : AbstractButton
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public ActionHandler Action;
    
    private readonly RequiredWatchedContainer<SubmitButton> SubmitContainer;
    
    public ServerActionButton(string text, ActionHandler action)
    {
        SubmitContainer = new(this, new(text));
        Action = action;
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
    public string Text
    {
        get => Submit.Text;
        set => Submit.Text = value;
    }
    
    public override string RenderedTag
        => "form";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            SubmitContainer
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-server-form"),
            ("method", "post"),
            ("enctype", "multipart/form-data"),
            ("action", "#")
        ];
}