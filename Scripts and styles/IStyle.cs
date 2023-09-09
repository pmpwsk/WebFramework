namespace uwap.WebFramework;

/// <summary>
/// Interface for styles for web pages (CSS).
/// </summary>
public interface IStyle
{
    /// <summary>
    /// Enumerates the style element's lines.
    /// </summary>
    public IEnumerable<string> Export(IRequest request);
}