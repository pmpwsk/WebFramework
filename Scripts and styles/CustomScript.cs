namespace uwap.WebFramework.Elements;

public class CustomScript : IScript
{
    public List<string> Lines;

    public CustomScript(string code)
        => Lines = code.Split('\n').ToList();

    public CustomScript(params string[] lines)
        => Lines = lines.ToList();

    public CustomScript(List<string> lines)
        => Lines = lines;

    public List<string> Export(IRequest request)
    {
        List<string> lines = new() { "<script>" };
        foreach (string line in Lines)
            lines.Add("\t" + line);
        lines.Add("</script>");
        return lines;
    }
}