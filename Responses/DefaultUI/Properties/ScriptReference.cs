namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to a JavaScript file.
/// </summary>
public class ScriptReference(Request req, string url) : AbstractResource(req, "src", url)
{
    public override string RenderedTag
        => "script";
}