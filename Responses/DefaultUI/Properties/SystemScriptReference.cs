namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI reference to the main default UI script.
/// </summary>
public class SystemScriptReference(Request req) : ScriptReference(req, $"{Server.Layers.SystemFilesLayerPrefix}/default-ui.js")
{
    public override string RenderedTag
        => "script";

    internal override string? FixedSystemId
        => "script";
}