using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI reference to a resource.
/// </summary>
public abstract class AbstractResource : WatchedElement
{
    private readonly RequiredWatchedAttribute LocationAttribute;
    
    protected AbstractResource(Request req, string attributeName, string url)
    {
        LocationAttribute = new(this, attributeName, Server.ResourcePath(req, url));
    }
    
    /// <summary>
    /// The URL of the resource.
    /// </summary>
    public string Location
        => LocationAttribute.Value;
    
    public void SetLocation(Request req, string url)
        => LocationAttribute.Value = Server.ResourcePath(req, url);
    
    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            LocationAttribute
        ];
}