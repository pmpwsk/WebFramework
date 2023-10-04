namespace uwap.WebFramework.Elements;

/// <summary>
/// Container element where the title and the button group are within an additional div.
/// </summary>
public class ContainerElementIsoTop : ContainerElement
{
    /// <summary>
    /// Creates a new isolated top container element without content.
    /// </summary>
    public ContainerElementIsoTop(string? title)
        : base(title) { }

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
            if (Large) yield return $"\t<h1>{Title}</h1>";
            else yield return $"\t<h2>{Title}</h2>";
        }

        if (Buttons.Any())
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

        if (Buttons.Any())
            yield return "\t<div class=\"clear\"></div>";

        yield return Closer;
    }
}