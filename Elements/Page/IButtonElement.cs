namespace uwap.WebFramework.Elements;

public abstract class IButtonElement : IPageElement
{
    protected override string ElementType => "a";

    public string? Text, Title;

    public override ICollection<string> Export()
    {
        List<string> lines = new List<string>();
        lines.Add(Opener);
        
        if (Title != null) lines.Add($"\t<h2>{Title}</h2>");
        if (Text != null) lines.Add($"\t<p>{Text}</p>");

        lines.Add(Closer);
        return lines;
    }
}