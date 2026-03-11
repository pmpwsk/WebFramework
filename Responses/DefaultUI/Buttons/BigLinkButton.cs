using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that navigates to a URL.
/// </summary>
public class BigLinkButton : OptionalIdElement
{
    private readonly RequiredWatchedAttribute TargetAttribute;
    
    private readonly OptionalWatchedAttribute NewTabAttribute;
    
    private readonly OptionalWatchedAttribute NoFollowAttribute;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    private readonly RequiredWatchedContainer<Heading3> HeaderContainer;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    public readonly ListWatchedContainer<Paragraph> Paragraphs;
    
    public BigLinkButton(IconAndText text, IEnumerable<string> subtexts, string target)
    {
        TargetAttribute = new(this, "href", target);
        NewTabAttribute = new(this, "target", null);
        NoFollowAttribute = new(this, "rel", null);
        var header = new Heading3(text);
        header.FixedAttributes.Remove(("class", "wf-heading"));
        HeaderContainer = new(this, header);
        Paragraphs = new(this, subtexts.Select(subtext => new Paragraph(subtext)));
        FixedAttributes.Add(("class", "wf-button wf-button-is-container"));
        FixedAttributes.Add(("draggable", "false"));
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public IconAndText Text
    {
        get => HeaderContainer.Element.Content;
        set => HeaderContainer.Element.Content = value;
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