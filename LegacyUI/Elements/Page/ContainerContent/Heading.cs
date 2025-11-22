namespace uwap.WebFramework.Elements;

/// <summary>
/// Additional heading for the content of a container (normal title size, even in LargeContainerElement parents).
/// </summary>
public class Heading : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "h2";

    //documentation inherited from IElement
    protected override string? ElementProperties => null;

    /// <summary>
    /// The heading text.
    /// </summary>
    public string Text;

    /// <summary>
    /// Creates a new heading content for a container.
    /// </summary>
    public Heading(string text, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Id = id;
        Class = classes;
        Style = styles;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener + Text.HtmlSafe(Unsafe) + Closer;
    }
}