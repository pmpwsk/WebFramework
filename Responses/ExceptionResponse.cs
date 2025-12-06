using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that show an error based on the given exception.
/// </summary>
public class ExceptionResponse(Exception exception) : IResponse
{
    public readonly Exception Exception = exception;
    
    public Task Respond(Request req, HttpContext context)
    {
        context.Response.StatusCode = 500;
        req.Exception = Exception;
        Presets.CreatePage(req, "Error", out var page);
        return new LegacyPageResponse(page, req).Respond(req, context);
    }
}