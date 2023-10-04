namespace uwap.WebFramework.Elements;

/// <summary>
/// Heading element (like LargeContainerElement but the buttons are at the bottom instead).
/// </summary>
public class HeadingElement : IContainerElement
{
    /// <summary>
    /// Creates a new heading element without content.
    /// </summary>
    public HeadingElement(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Creates a new heading element with the given text as a paragraph (use "" here to create a container without content).
    /// </summary>
    public HeadingElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (text != "") Contents.Add(new Paragraph(text));
    }

    /// <summary>
    /// Creates a new heading element with the given piece of content.
    /// </summary>
    public HeadingElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (content != null) Contents.Add(content);
    }

    /// <summary>
    /// Creates a new heading element with the given list of contents.
    /// </summary>
    public HeadingElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
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

        if (Title != null) yield return $"\t<h1>{Title}</h1>";
        
        foreach(IContent content in Contents)
            foreach (string line in content.Export())
                yield return $"\t{line}";

        if (Buttons.Any())
        {
            yield return "\t<div class=\"buttons\">";
            foreach(IButton button in Buttons)
                yield return "\t\t" + button.Export();
            yield return "\t</div>";

            yield return "\t<div class=\"clear\"></div>";
        }

        yield return Closer;
    }
}