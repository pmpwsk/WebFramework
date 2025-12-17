namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// A generic response to a UI action.
/// </summary>
public interface IActionResponse : IDisposable
{
    /// <summary>
    /// Generates the response object.
    /// </summary>
    public object Generate(Request req);
}