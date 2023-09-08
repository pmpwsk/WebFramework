namespace uwap.WebFramework.Elements;

public class CustomStyle : IStyle
{
    public List<string> Lines;

    public CustomStyle(string code)
        => Lines = code.Split('\n').ToList();

    public CustomStyle(params string[] lines)
        => Lines = lines.ToList();

    public CustomStyle(List<string> lines)
        => Lines = lines;

    public List<string> Export(IRequest request)
    {
        List<string> lines = new() { "<style>" };
        foreach (string line in Lines)
            lines.Add("\t" + line);
        lines.Add("</style>");
        return lines;
    }
}