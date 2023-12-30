namespace uwap.WebFramework.Elements;

/// <summary>
/// Regular container element.
/// </summary>
public class ContainerElement : IContainerElement
{
    /// <summary>
    /// Whether the title should be large or not (false here, only meant for overwriting by LargeContainerElement).
    /// </summary>
    protected virtual bool Large => false;

    /// <summary>
    /// Creates a new container element with the given pieces of content.
    /// </summary>
    public ContainerElement(string? title, params IContent[] contents)
    {
        Title = title;
        Class = null;
        Style = null;
        Id = null;
        Contents = [.. contents];
    }

    /// <summary>
    /// Creates a new container element with the given text as a paragraph (use "" here to create a container without content).
    /// </summary>
    public ContainerElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (text != "") Contents.Add(new Paragraph(text));
    }

    /// <summary>
    /// Creates a new container element with the given piece of content.
    /// </summary>
    public ContainerElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (content != null) Contents.Add(content);
    }

    /// <summary>
    /// Creates a new container element with the given paragraphs.
    /// </summary>
    public ContainerElement(string? title, IEnumerable<string> paragraphs, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        foreach (string p in paragraphs)
            Contents.Add(new Paragraph(p));
    }

    /// <summary>
    /// Creates a new container element with the given list of contents.
    /// </summary>
    public ContainerElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (contents != null) Contents = contents;
    }

    //documentation inherited from IPageElement
    public override IEnumerable<string> Export()
    {
        yield return Opener;

        if (Title != null)
        {
            if (Large) yield return $"\t<h1>{Title}</h1>";
            else yield return $"\t<h2>{Title}</h2>";
        }

        if (Buttons.Count != 0)
        {
            yield return "\t<div class=\"buttons\">";
            foreach (IButton button in Buttons)
                yield return "\t\t" + button.Export();
            yield return "\t</div>";
        }

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