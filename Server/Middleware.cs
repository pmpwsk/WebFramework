using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// Whether to block all incoming requests with a message (intended for when the server is updating or shutting down).
    /// </summary>
    public static bool PauseRequests { get; set; } = false;

    /// <summary>
    /// Middleware to attach handlers to ASP.NET.
    /// </summary>
    private class Middleware(RequestDelegate next)
    {
        private RequestDelegate Next = next;
        
        /// <summary>
        /// Invoked by ASP.NET for an incoming request with the given HttpContext.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                LayerRequestData data = new(context);
                foreach (LayerDelegate layer in Config.Layers)
                    if (await layer.Invoke(data))
                        return;

                if (Config.AllowMoreMiddlewaresIfUnhandled)
                    await Next.Invoke(context);
                else
                {
                    Request req = new(data);
                    req.Status = 501;
                    try { await req.Finish(); } catch { }
                }
            } catch { }
        }
    }

    /// <summary>
    /// Adds the headers for file serving (type, cache). If the browser already has the latest version (=abort), true is returned, otherwise false.
    /// </summary>
    private static bool AddFileHeaders(HttpContext context, string extension, string timestamp)
    {
        //content type
        if (Config.MimeTypes.TryGetValue(extension, out string? type)) context.Response.ContentType = type;

        //browser cache
        if (Config.BrowserCacheMaxAge.TryGetValue(extension, out int maxAge))
        {
            if (maxAge == 0)
                context.Response.Headers.CacheControl = "no-cache, private";
            else
            {
                context.Response.Headers.CacheControl = "public, max-age=" + maxAge;
                try
                {
                    if (context.Request.Headers.TryGetValue("If-None-Match", out var oldTag) && oldTag == timestamp)
                    {
                        context.Response.StatusCode = 304;
                        if (Config.FileCorsDomain != null)
                            context.Response.Headers.AccessControlAllowOrigin = Config.FileCorsDomain;
                        return true; //browser already has the current version
                    }
                    else context.Response.Headers.ETag = timestamp;
                }
                catch { }
            }
        }
        return false;
    }
}