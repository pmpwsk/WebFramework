using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form.
/// </summary>
public class SubmitButton : ButtonWithText
{
    private readonly OptionalWatchedAttribute OverrideActionAttribute;
    
    public SubmitButton(string text, string? overrideAction = null) : base(text)
    {
        OverrideActionAttribute = new(this, "formaction", overrideAction);
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

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("type", "submit")
        ];

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            OverrideActionAttribute
        ];
}