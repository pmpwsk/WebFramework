using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that navigates to a URL.
/// </summary>
public class LinkButton : ButtonWithText
{
    private readonly RequiredWatchedAttribute TargetAttribute;
    
    public LinkButton(string text, string target) : base(text)
    {
        TargetAttribute = new(this, "href", target);
        FixedAttributes.Add(("draggable", "false"));
    }
    
    /// <summary>
    /// The URL to navigate to.
    /// </summary>
    public string Target
    {
        get => TargetAttribute.Value;
        set => TargetAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "a";
}