using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI reference to a resource.
/// </summary>
public abstract class AbstractResource : OptionalIdElement
{
    private readonly RequiredWatchedAttribute LocationAttribute;
    
    protected AbstractResource(Request req, string attributeName, string url)
    {
        LocationAttribute = new(this, attributeName, Server.ResourcePath(req, url).GetAwaiter().GetResult());
    }
    
    /// <summary>
    /// The URL of the resource.
    /// </summary>
    public string Location
        => LocationAttribute.Value;
    
    public virtual async Task SetLocationAsync(Request req, string url)
        => LocationAttribute.Value = await Server.ResourcePath(req, url);
    
    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            LocationAttribute
        ];
}