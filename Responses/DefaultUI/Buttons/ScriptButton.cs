using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that runs JavaScript code.
/// </summary>
public class ScriptButton : ButtonWithText
{
    private readonly RequiredWatchedAttribute OnClickAttribute;
    
    public ScriptButton(string text, string onClick) : base(text)
    {
        OnClickAttribute = new(this, "onclick", onClick);
    }
    
    /// <summary>
    /// The JavaScript code to run.
    /// </summary>
    public string OnClick
    {
        get => OnClickAttribute.Value;
        set => OnClickAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "button";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("type", "button")
        ];

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            OnClickAttribute
        ];
}