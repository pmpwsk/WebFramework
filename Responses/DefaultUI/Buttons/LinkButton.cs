using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that navigates to a URL.
/// </summary>
public class LinkButton : ButtonWithText
{
    private readonly RequiredWatchedAttribute TargetAttribute;
    
    private readonly OptionalWatchedAttribute NewTabAttribute;
    
    private readonly OptionalWatchedAttribute NoFollowAttribute;
    
    public LinkButton(IconAndText content, string target) : base(content)
    {
        TargetAttribute = new(this, "href", target);
        NewTabAttribute = new(this, "target", null);
        NoFollowAttribute = new(this, "rel", null);
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
    
    /// <summary>
    /// Whether the link should be opened in a new tab.
    /// </summary>
    public bool NewTab
    {
        get => NewTabAttribute.Value != null;
        set => NewTabAttribute.Value = value ? "_blank" : null;
    }
    
    /// <summary>
    /// Whether the link should be followed by bots analyzing the page.
    /// </summary>
    public bool NoFollow
    {
        get => NoFollowAttribute.Value != null;
        set => NoFollowAttribute.Value = value ? "nofollow" : null;
    }
    
    public override string RenderedTag
        => "a";
}