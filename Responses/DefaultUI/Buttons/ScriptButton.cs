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
        FixedAttributes.Add(("type", "button"));
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
}