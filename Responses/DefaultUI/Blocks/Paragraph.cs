using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A paragraph.
/// </summary>
public class Paragraph : WatchedElement
{
    /// <summary>
    /// The paragraph's content parts.
    /// </summary>
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    public Paragraph(IEnumerable<AbstractMarkdownPart> content)
    {
        Content = new(this, content);
    }
    
    public Paragraph(params string[] lines)
        : this(LineBreak.Convert(lines))
    {
    }
    
    public void SetLines(params string[] lines)
        => Content.ReplaceAll(LineBreak.Convert(lines));
    
    public override string RenderedTag
        => "p";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            Content
        ];
}