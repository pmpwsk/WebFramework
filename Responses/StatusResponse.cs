using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that provides details for an HTTP status code.
/// </summary>
public class StatusResponse(int status) : IResponse
{
    public static readonly StatusResponse Success = new StatusResponse(200);
    
    public static readonly StatusResponse NotChanged = new StatusResponse(304);

    public static readonly StatusResponse BadRequest = new StatusResponse(400);

    public static readonly StatusResponse NotAuthenticated = new StatusResponse(401);

    public static readonly StatusResponse Forbidden = new StatusResponse(403);

    public static readonly StatusResponse NotFound = new StatusResponse(404);

    public static readonly StatusResponse BadMethod = new StatusResponse(405);

    public static readonly StatusResponse PayloadTooLarge = new StatusResponse(413);

    public static readonly StatusResponse Teapot = new StatusResponse(418);

    public static readonly StatusResponse TooManyRequests = new StatusResponse(429);

    public static readonly StatusResponse ServerError = new StatusResponse(500);

    public static readonly StatusResponse NotImplemented = new StatusResponse(501);

    public static readonly StatusResponse ServiceUnavailable = new StatusResponse(503);

    public static readonly StatusResponse InsufficientStorage = new StatusResponse(507);
    
    public readonly int Status = status;
    
    public Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.StatusCode = Status;
        Presets.CreatePage(req, Status.ToString(), out var page);
        return new LegacyPageResponse(page, req).Respond(req, context);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}