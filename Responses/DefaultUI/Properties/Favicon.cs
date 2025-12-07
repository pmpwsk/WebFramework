using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to an icon file.
/// </summary>
public class Favicon : AbstractResource
{
    private readonly OptionalWatchedAttribute TypeAttribute;
    
    public Favicon(Request req, string url) : base(req, "href", url)
    {
        TypeAttribute = new(this, "type", url.GetMimeType());
    }
    
    public override async Task SetLocationAsync(Request req, string url)
    {
        await base.SetLocationAsync(req, url);
        TypeAttribute.Value = url.GetMimeType();
    }
    
    public override string RenderedTag
        => "link";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("rel", "icon")
        ];

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            TypeAttribute
        ];
}