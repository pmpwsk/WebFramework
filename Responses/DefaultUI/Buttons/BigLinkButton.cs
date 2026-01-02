using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that navigates to a URL.
/// </summary>
public class BigLinkButton : OptionalIdElement
{
    private readonly RequiredWatchedAttribute TargetAttribute;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    private readonly RequiredWatchedContainer<Heading3> HeaderContainer;
    
    /// <summary>
    /// The object describing the button's header text.
    /// </summary>
    public readonly ListWatchedContainer<Paragraph> Paragraphs;
    
    public BigLinkButton(string text, IEnumerable<string> subtexts, string target)
    {
        TargetAttribute = new(this, "href", target);
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
    public string Text
    {
        get => HeaderContainer.Element.Text;
        set => HeaderContainer.Element.Text = value;
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