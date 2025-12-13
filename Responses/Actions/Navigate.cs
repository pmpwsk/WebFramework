namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// A response to a UI action that tells the client to navigate to another location.
/// </summary>
public class Navigate(string location) : IActionResponse
{
    public readonly string Location = location;
    
    public object Generate(Request req)
        => new { type = "Navigate", location = Location };

    public void Dispose()
        => GC.SuppressFinalize(this);
}