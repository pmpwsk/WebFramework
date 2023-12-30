namespace uwap.WebFramework.Elements;

/// <summary>
/// Text paragraph for a container.
/// </summary>
public class Paragraph : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "p";

    /// <summary>
    /// The text of the paragraph.
    /// </summary>
    public string Text;

    /// <summary>
    /// Creates a new paragraph for a container.
    /// </summary>
    public Paragraph(string text, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Class = classes;
        Style = styles;
        Id = id;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener + (Text??"") + Closer;
    }
}