namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to a CSS file.
/// </summary>
public class StyleReference : AbstractResource
{
    public StyleReference(Request req, string url) : base(req, "href", url)
    {
        FixedAttributes.Add(("rel", "stylesheet"));
        FixedAttributes.Add(("type", "text/css"));
        FixedAttributes.Add(("media", "screen"));
    }

    public override string RenderedTag
        => "link";
}