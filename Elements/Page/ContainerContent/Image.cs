namespace uwap.WebFramework.Elements;

public class Image : IContent
{
    protected override string ElementType => "img";
    protected override string? ElementProperties => $"src=\"{Source}\"{(Title==null?"":$"title=\"{Title}\"")}";

    public string Source;
    public string? Title;

    public Image(string source, string? styles, string? classes = null, string? id = null, string? title = null)
    {
        Source = source;
        Class = classes;
        Style = styles;
        Id = id;
        Title = title;
    }

    public override ICollection<string> Export()
        => new List<string> { CodeWithoutExplicitCloser };
}