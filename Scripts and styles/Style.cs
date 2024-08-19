namespace uwap.WebFramework.Elements;

/// <summary>
/// IScript that either transmits the code directly or references it, depending on the situation.
/// </summary>
/// <param name="flatten">Whether to write the style's code directly to the page instead of referencing it.</param>
public class Style(string url, bool flatten = false) : ScriptOrStyle(url, flatten), IStyle
{
    //documentation inherited from ScriptOrStyle
    protected override string BuildReference(string url) => $"<link rel=\"stylesheet\" type=\"text/css\" media=\"screen\" href=\"{url.HtmlValueSafe()}\">";

    //documentation inherited from ScriptOrStyle
    protected override string Tag => "style";

    //documentation inherited from ScriptOrStyle
    protected override string Extension => ".css";
}