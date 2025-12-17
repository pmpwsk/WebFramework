using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form while overriding the action with an action executed on the server.
/// </summary>
public class ServerSubmitButton : AbstractButton, IActionHaver
{
    public ActionHandler Action { get; set; }
    
    private readonly RequiredWatchedAttribute TextAttribute;
    
    public ServerSubmitButton(string text, ActionHandler action)
    {
        TextAttribute = new(this, "value", text);
        Action = action;
        FixedAttributes.Add(("class", "wf-button wf-server-form-override"));
        FixedAttributes.Add(("type", "submit"));
    }
    
    public override string RenderedTag
        => "input";
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public string Text
    {
        get => TextAttribute.Value;
        set => TextAttribute.Value = value;
    }

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