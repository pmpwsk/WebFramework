namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract class for attributes that are being watched for changes.
/// </summary>
public abstract class AbstractWatchedAttribute(WatchedElement parent, string name)
{
    /// <summary>
    /// The parent element.
    /// </summary>
    public readonly WatchedElement Parent = parent;
    
    /// <summary>
    /// The attribute's rendered name.
    /// </summary>
    public readonly string Name = name;
    
    /// <summary>
    /// Exports the attribute as a name-value pair that can be rendered.
    /// </summary>
    public abstract (string Name, string? Value) Build();
}