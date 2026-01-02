using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

public enum ColorSchemeOption
{
    Light,
    Dark,
    LightOrDark,
    DarkOrLight
}

/// <summary>
/// A color scheme metadata tag.
/// </summary>
public class ColorScheme : WatchedElement
{
    private RequiredWatchedAttribute ValueAttribute;
    
    public ColorScheme(ColorSchemeOption value)
    {
        FixedAttributes.Add(("name", "color-scheme"));
        ValueAttribute = new(this, "content", ToString(value));
    }
    
    public ColorSchemeOption Value
    {
        get => FromString(ValueAttribute.Value);
        set => ValueAttribute.Value = ToString(value);
    }
    
    public override string RenderedTag
        => "meta";
    
    private static string ToString(ColorSchemeOption value)
        => value switch
        {
            ColorSchemeOption.Light => "light",
            ColorSchemeOption.Dark => "dark",
            ColorSchemeOption.LightOrDark => "light dark",
            ColorSchemeOption.DarkOrLight => "dark light",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    
    private static ColorSchemeOption FromString(string value)
        => value switch
        {
            "light" => ColorSchemeOption.Light,
            "dark" => ColorSchemeOption.Dark,
            "light dark" => ColorSchemeOption.LightOrDark,
            "dark light" => ColorSchemeOption.DarkOrLight,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
}