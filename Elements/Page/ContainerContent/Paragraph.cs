namespace uwap.WebFramework.Elements;

public class Paragraph : IContent
{
    protected override string ElementType => "p";

    public string Text;

    public Paragraph(string text, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Class = classes;
        Style = styles;
        Id = id;
    }

    public override ICollection<string> Export()
        => new List<string> { Opener + Text + Closer };
}