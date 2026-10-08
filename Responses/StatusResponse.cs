using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that provides details for an HTTP status code.
/// </summary>
public class StatusResponse(int status) : IResponse
{
    public static StatusResponse Success => new(200);
    
    public static StatusResponse NotChanged => new(304);

    public static StatusResponse BadRequest => new(400);

    public static StatusResponse NotAuthenticated => new(401);

    public static StatusResponse Forbidden => new(403);

    public static StatusResponse NotFound => new(404);

    public static StatusResponse BadMethod => new(405);

    public static StatusResponse PayloadTooLarge => new(413);

    public static StatusResponse Teapot => new(418);

    public static StatusResponse TooManyRequests => new(429);

    public static StatusResponse ServerError => new(500);

    public static StatusResponse NotImplemented => new(501);

    public static StatusResponse ServiceUnavailable => new(503);

    public static StatusResponse InsufficientStorage => new(507);
    
    public readonly int Status = status;
    
    public async Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.StatusCode = Status;
        if (req.Method == "GET")
        {
            Presets.CreatePage(req, Status.ToString(), out var page);
            using var response = new LegacyPageResponse(page, req);
            await response.Respond(req, context);
        }
        else
        {
            using var response = new DummyResponse();
            await response.Respond(req, context);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}