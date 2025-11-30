using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A list item.
/// </summary>
public class ListItem : WatchedElement
{
    /// <summary>
    /// The paragraph's content parts.
    /// </summary>
    public readonly ListWatchedContainer<AbstractMarkdownPart> Content;
    
    /// <summary>
    /// The list item's list of child items.
    /// </summary>
    public readonly ListWatchedContainer<AbstractList> Sublists;
    
    public ListItem(IEnumerable<AbstractMarkdownPart> content, IEnumerable<AbstractList>? sublists = null)
    {
        Content = new(this, content);
        Sublists = new(this, sublists ?? []);
    }
    
    public ListItem(params string[] lines)
        : this(LineBreak.Convert(lines))
    {
    }
    
    public void SetLines(params string[] lines)
        => Content.ReplaceAll(LineBreak.Convert(lines));
    
    public override string RenderedTag
        => "li";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            Content,
            Sublists
        ];
}