namespace uwap.WebFramework.Elements;

public class FileSelector : IContent
{
    protected override string ElementType => "";
    
    public FileSelector(string id)
    {
        Id = id;
    }

    public override ICollection<string> Export()
    {
        return new List<string> { $"<input type=\"file\" id=\"{Id}\">" };
    }
}