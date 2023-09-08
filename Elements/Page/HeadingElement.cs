namespace uwap.WebFramework.Elements;

public class HeadingElement : IContainerElement
{
    public HeadingElement(string title)
    {
        Title = title;
    }

    public HeadingElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (text != "") Contents.Add(new Paragraph(text));
    }

    public HeadingElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (content != null) Contents.Add(content);
    }

    public HeadingElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (contents != null) Contents = contents;
    }

    public override ICollection<string> Export()
    {
        List<string> e = new List<string>();
        e.Add(Opener);

        if (Title != null) e.Add($"\t<h1>{Title}</h1>");
        
        foreach(IContent content in Contents)
            foreach (string line in content.Export())
                e.Add($"\t{line}");

        if (Buttons.Count != 0)
        {
            e.Add("\t<div class=\"buttons\">");
            foreach(IButton button in Buttons)
                e.Add("\t\t" + button.Export());
            e.Add("\t</div>");
        }

        e.Add(Closer);
        return e;
    }
}