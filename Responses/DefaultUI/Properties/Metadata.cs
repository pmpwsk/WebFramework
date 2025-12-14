using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A metadata tag.
/// </summary>
public class Metadata : WatchedElement
{
    private readonly RequiredWatchedAttribute NameAttribute;
    
    private readonly RequiredWatchedAttribute ContentAttribute;
    
    public Metadata(string name, string content)
    {
        NameAttribute = new(this, "name", name);
        ContentAttribute = new(this, "content", content);
    }
    
    /// <summary>
    /// The metadata's name.
    /// </summary>
    public string Name
    {
        get => NameAttribute.Value;
        set => NameAttribute.Value = value;
    }
    
    /// <summary>
    /// The metadata's value.
    /// </summary>
    public string Content
    {
        get => ContentAttribute.Value;
        set => ContentAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "meta";
}