namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// An empty response to a UI action.
/// </summary>
public class Nothing : IActionResponse
{
    /// <summary>
    /// An action handler that does nothing.
    /// </summary>
    public static Task<IActionResponse> EmptyHandler(Request req)
        => Task.FromResult<IActionResponse>(new Nothing());
    
    public object Generate(Request req)
        => new { type = "Nothing" };

    public void Dispose()
        => GC.SuppressFinalize(this);
}