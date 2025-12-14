using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// An abstract HTML element with common properties and an optional ID.
/// </summary>
public abstract class OptionalIdElement : CommonElement
{
    /// <summary>
    /// The element's ID.
    /// </summary>
    private readonly OptionalWatchedAttribute IdAttribute;
    
    protected OptionalIdElement()
    {
        IdAttribute = new(this, "id", null);
    }
    
    /// <summary>
    /// The element's ID.
    /// </summary>
    public string? Id
    {
        get => IdAttribute.Value;
        set => IdAttribute.Value = value;
    }
}