using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form.
/// </summary>
public class SubmitButton : ButtonWithText
{
    private readonly OptionalWatchedAttribute OverrideActionAttribute;
    
    public SubmitButton(IconAndText content, string? overrideAction = null) : base(content)
    {
        OverrideActionAttribute = new(this, "formaction", overrideAction);
        FixedAttributes.Add(("class", "wf-button"));
        FixedAttributes.Add(("type", "submit"));
    }
    
    public override string RenderedTag
        => "button";
    
    /// <summary>
    /// The URL of the action to use instead of the form's action.
    /// </summary>
    public string? OverrideAction
    {
        get => OverrideActionAttribute.Value;
        set => OverrideActionAttribute.Value = value;
    }
}