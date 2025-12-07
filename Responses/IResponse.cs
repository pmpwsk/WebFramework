using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A generic response to a request.
/// </summary>
public interface IResponse : IDisposable
{
    /// <summary>
    /// Sends the response to the given request.
    /// </summary>
    public Task Respond(Request req, HttpContext context);
}