namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// A raw markdown text.
/// </summary>
public class MarkdownText(string text) : AbstractMarkdownPart
{
    /// <summary>
    /// The text content.
    /// </summary>
    public readonly string Text = text;
    
    public override IEnumerable<string> EnumerateChunks()
    {
        yield return Text.HtmlSafe();
    }
}