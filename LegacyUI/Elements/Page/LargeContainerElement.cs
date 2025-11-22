namespace uwap.WebFramework.Elements;

/// <summary>
/// Container element with a larger title (like Heading, but Heading has the buttons at the bottom).
/// </summary>
public class LargeContainerElement : ContainerElement
{
    //documentation inherited from ContainerElement
    protected override bool Large => true;

    /// <summary>
    /// Creates a new large container element with the given pieces of content.
    /// </summary>
    public LargeContainerElement(string? title, params IContent[] contents)
        : base(title, contents) { }

    /// <summary>
    /// Creates a new large container element with the given text as a paragraph.
    /// </summary>
    public LargeContainerElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
        : base(title, text, classes, styles, id) { }

    /// <summary>
    /// Creates a new large container element with the given piece of content.
    /// </summary>
    public LargeContainerElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
        : base(title, content, classes, styles, id) { }

    /// <summary>
    /// Creates a new large container element with the given paragraphs.
    /// </summary>
    public LargeContainerElement(string? title, IEnumerable<string> paragraphs, string? classes = null, string? styles = null, string? id = null)
        : base(title, paragraphs, classes, styles, id) { }

    /// <summary>
    /// Creates a new large container element with the given list of contents.
    /// </summary>
    public LargeContainerElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
        : base(title, contents, classes, styles, id) { }
}