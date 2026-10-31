using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that does absolutely nothing.
/// </summary>
public class DummyResponse : IResponse
{
    public Task Respond(Request req, HttpContext context)
    {
        req.Cookies.DisableWriting();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}