using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form while overriding the action with an action executed on the server.
/// </summary>
public class ServerSubmitButton : AbstractButton
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public Func<IResponse> Action = () => StatusResponse.Success;
    
    private readonly RequiredWatchedAttribute TextAttribute;
    
    public ServerSubmitButton(string text)
    {
        TextAttribute = new(this, "value", text);
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

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            TextAttribute
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-button wf-server-form-override"),
            ("type", "submit")
        ];
}