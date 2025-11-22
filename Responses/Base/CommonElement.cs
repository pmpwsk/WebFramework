using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// An abstract HTML element with common properties (class, style and custom attributes)
/// </summary>
public abstract class CommonElement : WatchedElement
{
    private readonly OptionalWatchedAttribute ClassAttribute;
    
    private readonly OptionalWatchedAttribute StyleAttribute;
    
    /// <summary>
    /// The custom attributes.
    /// </summary>
    public readonly ListWatchedAttributes Attributes;
    
    protected CommonElement()
    {
        ClassAttribute = new(this, "class", null);
        StyleAttribute = new(this, "style", null);
        Attributes = new(this, []);
    }
    
    /// <summary>
    /// The element's CSS classes.
    /// </summary>
    public string? Class
    {
        get => ClassAttribute.Value;
        set => ClassAttribute.Value = value;
    }
    
    /// <summary>
    /// The element's CSS settings.
    /// </summary>
    public string? Style
    {
        get => StyleAttribute.Value;
        set => StyleAttribute.Value = value;
    }

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            ClassAttribute,
            StyleAttribute
        ];

    public override IEnumerable<(string Name, string? Value)> RenderedAttributes
        => [
            ..base.RenderedAttributes,
            ..Attributes
        ];
}