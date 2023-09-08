namespace uwap.WebFramework.Elements;

public class UList : IContent
{
    protected override string ElementType => "ul";

    public List<string> List;

    public UList(List<string> list, string? classes = null, string? styles = null, string? id = null)
    {
        List = list;
        Class = classes;
        Style = styles;
        Id = id;
    }

    public UList(params string[] items)
    {
        List = items.ToList();
        Class = null;
        Style = null;
        Id = null;
    }

    public override ICollection<string> Export()
    {
        List<string> result = new List<string>();
        result.Add(Opener);

        foreach (string item in List)
            result.Add($"\t<li>{item}</li>");

        result.Add(Closer);
        return result;
    }
}