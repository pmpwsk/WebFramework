namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An unordered list.
/// </summary>
public class BulletList(IEnumerable<ListItem> items) : AbstractList(items)
{
    public override string RenderedTag
        => "ul";
}