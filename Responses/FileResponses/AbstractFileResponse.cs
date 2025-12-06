using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// An abstract file response.
/// </summary>
public abstract class AbstractFileResponse(bool allowCors, string? timestamp) : IResponse
{
    protected readonly bool AllowCors = allowCors;
    
    protected readonly string? Timestamp = timestamp;
    
    public string? FixedType = null;
    
    public bool Revalidate = false;
    
    public string? ContentSecurityPolicy = null;
    
    protected abstract string? Extension { get; }
    
    protected abstract long? Length { get; }
    
    protected abstract Task WriteTo(HttpContext context);
    
    public Task Respond(Request req, HttpContext context)
    {
        // CORS domain
        if (AllowCors)
            context.SetCorsDomain(Server.Config.FileCorsDomain);
        
        if (Extension != null)
        {
            // MIME type
            if (FixedType != null)
                context.Response.ContentType = FixedType;
            else if (Server.Config.MimeTypes.TryGetValue(Extension, out string? type))
                context.Response.ContentType = type;
            
            // Cache duration
            if (Revalidate && Timestamp != null)
                context.Response.Headers.CacheControl = "no-cache, private, must-revalidate";
            else if (Server.Config.BrowserCacheMaxAge.TryGetValue(Extension, out int maxAge))
                if (maxAge == 0)
                    context.Response.Headers.CacheControl = "no-cache, private";
                else
                    context.Response.Headers.CacheControl = "public, max-age=" + maxAge;
        }
        
        // CSP
        if (ContentSecurityPolicy != null)
            context.Response.Headers.ContentSecurityPolicy = ContentSecurityPolicy;
                
        // Timestamp
        if (Timestamp != null)
        {
            if (context.Request.Headers.TryGetValue("If-None-Match", out var oldTag) && oldTag == Timestamp)
            {
                // Browser already has the current version
                StatusResponse.NotChanged.Respond(req, context);
                return Task.CompletedTask;
            }
            else
                context.Response.Headers.ETag = Timestamp;
        }
        
        // Content length
        if (Length != null)
            context.Response.ContentLength = Length;
        
        // Body
        return WriteTo(context);
    }
}