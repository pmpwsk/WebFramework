namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// A response to a UI action that tells the client to reload the page.
/// </summary>
public class Reload : IActionResponse
{
    public object Generate(Request req)
        => new { type = "Reload" };

    public void Dispose()
        => GC.SuppressFinalize(this);
}