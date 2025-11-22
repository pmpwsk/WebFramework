namespace uwap.WebFramework.Elements;

/// <summary>
/// Image for a container.
/// </summary>
public class Image : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "img";

    //documentation inherited from IElement
    protected override string? ElementProperties => $"src=\"{Source.HtmlValueSafe()}\"{(Title==null?"":$" title=\"{Title.HtmlValueSafe()}\"")}";

    /// <summary>
    /// Source value for the image. This could be a URL or the image as base64 data with information.
    /// </summary>
    public string Source;

    /// <summary>
    /// The text that will appear as a tooltip when hovering over the image.
    /// </summary>
    public string? Title;

    /// <summary>
    /// Creates a new image for a container.
    /// </summary>
    public Image(string source, string? styles, string? classes = null, string? id = null, string? title = null)
    {
        Source = source;
        Class = classes;
        Style = styles;
        Id = id;
        Title = title;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return CodeWithoutExplicitCloser;
    }
}