namespace uwap.WebFramework.Elements;

public class Style : ScriptOrStyle, IStyle
{
    protected override string LinkCode(string url) => $"<link rel=\"stylesheet\" type=\"text/css\" media=\"screen\" href=\"{url}\">";
    protected override string Tag => "style";
    protected override string Extension => ".css";

    public Style(string url) : base(url) { }
}