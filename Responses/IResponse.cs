namespace uwap.WebFramework.Responses;

/// <summary>
/// A generic response to a request.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Sends the response to the given request.
    /// </summary>
    public Task Respond(Request req);
}