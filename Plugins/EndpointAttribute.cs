namespace uwap.WebFramework.Plugins;

/// <summary>
/// An attribute to declare handler methods in plugins as endpoints.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EndpointAttribute(string path) : Attribute
{
    /// <summary>
    /// The path that is handled by the endpoint.
    /// </summary>
    public readonly string Path = path;
}