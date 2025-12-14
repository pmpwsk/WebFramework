using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to the canonical origin of the page.
/// </summary>
public class CanonicalReference : OptionalIdElement
{
    private readonly RequiredWatchedAttribute LocationAttribute;
    
    public CanonicalReference(string url)
    {
        LocationAttribute = new(this, "href", url);
        FixedAttributes.Add(("rel", "canonical"));
    }
    
    public override string RenderedTag
        => "link";
    
    /// <summary>
    /// The URL of the page's origin.
    /// </summary>
    public string Location
    {
        get => LocationAttribute.Value;
        set => LocationAttribute.Value = value;
    }
}