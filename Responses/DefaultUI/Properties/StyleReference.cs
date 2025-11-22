namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to a CSS file.
/// </summary>
public class StyleReference(Request req, string url) : AbstractResource(req, "href", url)
{
    public override string RenderedTag
        => "link";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("rel", "stylesheet"),
            ("type", "text/css"),
            ("media", "screen")
        ];
}