namespace uwap.WebFramework.Elements;

/// <summary>
/// Container element with a large title where the title and the button group are within an additional div.
/// </summary>
public class LargeContainerElementIsoTop : ContainerElementIsoTop
{
    //documentation inherited from ContainerElement
    protected override bool Large => true;

    /// <summary>
    /// Creates a new large-titled isolated top container element without content.
    /// </summary>
    public LargeContainerElementIsoTop(string? title)
        : base(title) { }

    /// <summary>
    /// Creates a new large-titled isolated top container element with the given text as a paragraph (use "" here to create a container without content).
    /// </summary>
    public LargeContainerElementIsoTop(string? title, string text, string? classes = null, string? styles = null, string? id = null)
        : base(title, text, classes, styles, id) { }

    /// <summary>
    /// Creates a new large-titled isolated top container element with the given piece of content.
    /// </summary>
    public LargeContainerElementIsoTop(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
        : base(title, content, classes, styles, id) { }

    /// <summary>
    /// Creates a new large-titled isolated top container element with the given list of contents.
    /// </summary>
    public LargeContainerElementIsoTop(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
        : base(title, contents, classes, styles, id) { }
}