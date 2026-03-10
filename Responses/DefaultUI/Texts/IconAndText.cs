namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A data structure for an optional icon and text, where at least one must be provided.
/// </summary>
public class IconAndText
{
    public readonly string? Icon;
    
    public readonly string? Text;

    /// <summary>
    /// A data structure for an optional icon and text, where at least one must be provided.
    /// </summary>
    public IconAndText(string? icon, string? text)
    {
        if (icon == null && text == null)
            throw new ArgumentException("Either icon or text must be specified.");
        
        Icon = icon;
        Text = text;
    }

    internal string GeneratedText
        => (Icon == null ? "" : " ") + Text;
    
    public static implicit operator IconAndText(string text) => new(null, text);
}