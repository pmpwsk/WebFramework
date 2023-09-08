namespace uwap.WebFramework.Elements;

public class Heading : IContent
{
    protected override string ElementType => "h2";
    protected override string? ElementProperties => null;

    public string Text;

    public Heading(string text, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Id = id;
        Class = classes;
        Style = styles;
    }

    public override ICollection<string> Export()
        => new List<string> { Opener + (Text) + Closer };
}