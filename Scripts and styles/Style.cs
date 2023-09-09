namespace uwap.WebFramework.Elements;

/// <summary>
/// IScript that either transmits the code directly or references it, depending on the situation.
/// </summary>
public class Style : ScriptOrStyle, IStyle
{
    //documentation inherited from ScriptOrStyle
    protected override string BuildReference(string url) => $"<link rel=\"stylesheet\" type=\"text/css\" media=\"screen\" href=\"{url}\">";

    //documentation inherited from ScriptOrStyle
    protected override string Tag => "style";

    //documentation inherited from ScriptOrStyle
    protected override string Extension => ".css";

    /// <summary>
    /// Creates a new style object for the style at the given URL.
    /// </summary>
    public Style(string url) : base(url) { }
}