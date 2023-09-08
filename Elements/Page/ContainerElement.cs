namespace uwap.WebFramework.Elements;

public class ContainerElement : IContainerElement
{
    protected virtual bool Large => false;

    public ContainerElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (text != "") Contents.Add(new Paragraph(text));
    }

    public ContainerElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Class = classes;
        Style = styles;
        Id = id;
        if (content != null) Contents.Add(content);
    }

    public ContainerElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
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

        e.Add("\t<div>"); //these two are for flex, evaluate if this is necessary!
        if (Title != null)
        {
            if (Large) e.Add($"\t<h1>{Title}</h1>");
            else e.Add($"\t<h2>{Title}</h2>");
        }

        if (Buttons.Count != 0)
        {
            e.Add("\t<div class=\"buttons\">");
            foreach (IButton button in Buttons)
                e.Add("\t\t" + button.Export());
            e.Add("\t</div>");
        }
        e.Add("\t</div>"); //the second part for flex

        foreach (IContent content in Contents)
            foreach (string line in content.Export())
                e.Add($"\t{line}");

        e.Add(Closer);
        return e;
    }
}