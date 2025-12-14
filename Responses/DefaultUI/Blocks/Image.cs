using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A link that navigates to a URL.
/// </summary>
public class Image : AbstractResource
{
    private readonly OptionalWatchedAttribute TitleAttribute;
    
    public Image(Request req, string url, string? title = null) : base(req, "src", url)
    {
        TitleAttribute = new(this, "title", title);
        FixedAttributes.Add(("class", "wf-image"));
    }
        
    public override string RenderedTag
        => "img";
}