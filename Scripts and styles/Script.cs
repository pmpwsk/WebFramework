namespace uwap.WebFramework.Elements;

public class Script : ScriptOrStyle, IScript
{
    protected override string LinkCode(string url) => $"<script src=\"{url}\"></script>";
    protected override string Tag => "script";
    protected override string Extension => ".js";

    public Script(string url) : base(url) { }
}