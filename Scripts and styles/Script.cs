namespace uwap.WebFramework.Elements;

/// <summary>
/// IScript that either transmits the code directly or references it, depending on the situation.
/// </summary>
/// <param name="flatten">Whether to write the script's code directly to the page instead of referencing it.</param>
public class Script(string url, bool flatten = false) : ScriptOrStyle(url, flatten), IScript
{
    //documentation inherited from ScriptOrStyle
    protected override string BuildReference(string url) => $"<script src=\"{url}\"></script>";

    //documentation inherited from ScriptOrStyle
    protected override string Tag => "script";

    //documentation inherited from ScriptOrStyle
    protected override string Extension => ".js";
}