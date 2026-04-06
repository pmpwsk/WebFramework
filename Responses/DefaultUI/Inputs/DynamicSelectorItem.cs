namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An item for <c>DynamicSelector</c>.
/// </summary>
public class DynamicSelectorItem<T>(T value, string name, string? description)
{
    public readonly T Value = value;
    
    public readonly string Name = name;
    
    public readonly string? Description = description;
}