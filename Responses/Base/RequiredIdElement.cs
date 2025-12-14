using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// An abstract HTML element with common properties and a required ID.
/// </summary>
public abstract class RequiredIdElement : CommonElement
{
    /// <summary>
    /// The element's ID.
    /// </summary>
    private readonly RequiredWatchedAttribute IdAttribute;
    
    protected RequiredIdElement(string id)
    {
        IdAttribute = new(this, "id", id);
    }
    
    /// <summary>
    /// The element's ID.
    /// </summary>
    public string Id
    {
        get => IdAttribute.Value;
        set => IdAttribute.Value = value;
    }
}