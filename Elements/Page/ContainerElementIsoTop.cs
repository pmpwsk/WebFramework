namespace uwap.WebFramework.Elements;

/// <summary>
/// Container element where the title and the button group are within an additional div.
/// </summary>
public class ContainerElementIsoTop : ContainerElement
{
    /// <summary>
    /// Creates a new isolated top container element with the given pieces of content.
    /// </summary>
    public ContainerElementIsoTop(string? title, params IContent[] contents)
        : base(title, contents) { }

    /// <summary>
    /// Creates a new isolated top container element with the given text as a paragraph (use "" here to create a container without content).
    /// </summary>
    public ContainerElementIsoTop(string? title, string text, string? classes = null, string? styles = null, string? id = null)
        : base(title, text, classes, styles, id) { }

    /// <summary>
    /// Creates a new isolated top container element with the given piece of content.
    /// </summary>
    public ContainerElementIsoTop(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
        : base(title, content, classes, styles, id) { }

    /// <summary>
    /// Creates a new isolated top container element with the given paragraphs.
    /// </summary>
    public ContainerElementIsoTop(string? title, IEnumerable<string> paragraphs, string? classes = null, string? styles = null, string? id = null)
        : base(title, paragraphs, classes, styles, id) { }

    /// <summary>
    /// Creates a new isolated top container element with the given list of contents.
    /// </summary>
    public ContainerElementIsoTop(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
        : base(title, contents, classes, styles, id) { }

    //documentation inherited from IPageElement
    public override IEnumerable<string> Export()
    {
        yield return Opener;

        yield return "\t<div>"; //this is additional
        if (Title != null)
        {
            if (Large)
                yield return $"\t<h1>{Title.HtmlSafe(Unsafe)}</h1>";
            else yield return $"\t<h2>{Title.HtmlSafe(Unsafe)}</h2>";
        }

        if (Buttons.Count != 0)
        {
            yield return "\t<div class=\"buttons\">";
            foreach (IButton button in Buttons)
                yield return "\t\t" + button.Export();
            yield return "\t</div>";
        }
        yield return "\t</div>"; //this is the closer for the additional div

        foreach (IContent content in Contents)
            foreach (string line in content.Export())
                yield return $"\t{line}";

        if (Buttons.Count != 0)
        {
            if (Title == null && ((Contents.Count == 0) || (Contents.Count == 1 && Contents.First() is Paragraph paragraph && paragraph.Text.Length <= 20)))
                yield return "\t<div class=\"clear-o\"></div>";
            else yield return "\t<div class=\"clear\"></div>";
        }

        yield return Closer;
    }
}