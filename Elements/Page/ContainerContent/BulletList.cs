namespace uwap.WebFramework.Elements;

/// <summary>
/// Unordered bullet list for a container.
/// </summary>
public class BulletList : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "ul";

    /// <summary>
    /// The list of items.
    /// </summary>
    public List<string> List;

    /// <summary>
    /// Creates a new unordered list for a container with the given items.
    /// </summary>
    public BulletList(List<string> list, string? classes = null, string? styles = null, string? id = null)
    {
        List = list;
        Class = classes;
        Style = styles;
        Id = id;
    }

    /// <summary>
    /// Creates a new unordered list for a container with the given items.
    /// </summary>
    public BulletList(params string[] items)
    {
        List = [.. items];
        Class = null;
        Style = null;
        Id = null;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener;

        foreach (string item in List)
            yield return $"\t<li>{item.HtmlSafe()}</li>";

        yield return Closer;
    }
}