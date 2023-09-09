namespace uwap.WebFramework.Elements;

/// <summary>
/// IScript that either transmits the code directly or references it, depending on the situation.
/// </summary>
public class Script : ScriptOrStyle, IScript
{
    //documentation inherited from ScriptOrStyle
    protected override string BuildReference(string url) => $"<script src=\"{url}\"></script>";

    //documentation inherited from ScriptOrStyle
    protected override string Tag => "script";

    //documentation inherited from ScriptOrStyle
    protected override string Extension => ".js";

    /// <summary>
    /// Creates a new script object for the style at the given URL.
    /// </summary>
    public Script(string url) : base(url) { }
}