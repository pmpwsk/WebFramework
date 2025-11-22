using uwap.WebFramework.Responses.Base;
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
    
    public Paragraph(List<AbstractMarkdownPart> content)
    {
        Content = new(this, content);
    }
    
    public Paragraph(params string[] lines)
        : this(Convert(lines))
    {
    }
    
    public void SetLines(params string[] lines)
        => Content.ReplaceAll(Convert(lines));
    
    public override string RenderedTag
        => "p";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            Content
        ];

    private static List<AbstractMarkdownPart> Convert(string[] lines)
    {
        List<AbstractMarkdownPart> parts = [];
        foreach (var line in lines)
        {
            if (parts.Count != 0)
                parts.Add(new LineBreak());
            
            parts.Add(new MarkdownText(line));
        }
        return parts;
    }
}