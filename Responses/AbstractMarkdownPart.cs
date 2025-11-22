namespace uwap.WebFramework.Responses;

/// <summary>
/// An abstract class that can be exported into chunks of text.
/// </summary>
public abstract class AbstractMarkdownPart
{
    /// <summary>
    /// Exports the object as chunks of text.
    /// </summary>
    public abstract IEnumerable<string> EnumerateChunks();
}