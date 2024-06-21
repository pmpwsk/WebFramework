namespace uwap.WebFramework;

/// <summary>
/// Interface for scripts for web pages (JavaScript).
/// </summary>
public interface IScript
{
    /// <summary>
    /// Enumerates the script element's lines.
    /// </summary>
    public IEnumerable<string> Export(Request req);
}