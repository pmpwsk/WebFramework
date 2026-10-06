using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form while overriding the action with an action executed on the server.
/// </summary>
public class ServerSubmitButton : ButtonWithText, IActionHaver
{
    public ActionHandlerAsync Action { get; set; }
    
    public ServerSubmitButton(IconAndText content, ActionHandlerAsync action) : base(content)
    {
        Action = action;
        FixedAttributes.Add(("class", "wf-button wf-server-form-override"));
        FixedAttributes.Add(("type", "submit"));
    }
    
    public ServerSubmitButton(IconAndText content, ActionHandler action)
        : this(content, action.ToAsync()) { }
    
    public override string RenderedTag
        => "button";

    /// <summary>
    /// Finds the form this element is part of.
    /// </summary>
    public WatchedElement? FindForm()
    {
        var parent = ParentContainer?.Parent;
        while (parent != null)
            if (parent is WatchedElement watchedElement)
                if (watchedElement.RenderedTag == "form")
                    return watchedElement;
                else
                    parent = watchedElement.ParentContainer?.Parent;
            else
                return null;

        return null;
    }
}