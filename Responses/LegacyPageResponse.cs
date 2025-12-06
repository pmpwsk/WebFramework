using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that sends a legacy page.
/// </summary>
public class LegacyPageResponse(IPage page, Request creatingRequest) : AbstractTextResponse
{
    private readonly IPage Page = page;
    
    private readonly Request Request = creatingRequest;
    
    public string? ContentSecurityPolicy = null;

    public override Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.ContentType = "text/html;charset=utf-8";
        if (ContentSecurityPolicy != null)
            context.Response.Headers.ContentSecurityPolicy = ContentSecurityPolicy;
        return base.Respond(req, context);
    }

    public override IEnumerable<string> EnumerateChunks()
        => Page.Export(Request).Select(line => line + "\n");
}