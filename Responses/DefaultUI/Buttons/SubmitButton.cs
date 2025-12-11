using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form.
/// </summary>
public class SubmitButton : AbstractButton
{
    private readonly OptionalWatchedAttribute OverrideActionAttribute;
    
    private readonly RequiredWatchedAttribute TextAttribute;
    
    public SubmitButton(string text, string? overrideAction = null)
    {
        OverrideActionAttribute = new(this, "formaction", overrideAction);
        TextAttribute = new(this, "value", text);
    }
    
    public override string RenderedTag
        => "input";
    
    /// <summary>
    /// The URL of the action to use instead of the form's action.
    /// </summary>
    public string? OverrideAction
    {
        get => OverrideActionAttribute.Value;
        set => OverrideActionAttribute.Value = value;
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public string Text
    {
        get => TextAttribute.Value;
        set => TextAttribute.Value = value;
    }

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            OverrideActionAttribute,
            TextAttribute
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-button"),
            ("type", "submit")
        ];
}