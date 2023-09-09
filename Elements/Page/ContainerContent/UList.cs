namespace uwap.WebFramework.Elements;

/// <summary>
/// Unsorted list for a container.
/// </summary>
public class UList : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "ul";

    /// <summary>
    /// The list of items.
    /// </summary>
    public List<string> List;

    /// <summary>
    /// Creates a new unsorted list for a container with the given items.
    /// </summary>
    public UList(List<string> list, string? classes = null, string? styles = null, string? id = null)
    {
        List = list;
        Class = classes;
        Style = styles;
        Id = id;
    }

    /// <summary>
    /// Creates a new unsorted list for a container with the given items.
    /// </summary>
    public UList(params string[] items)
    {
        List = items.ToList();
        Class = null;
        Style = null;
        Id = null;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener;

        foreach (string item in List)
            yield return $"\t<li>{item}</li>";

        yield return Closer;
    }
}