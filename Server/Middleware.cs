using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Database;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public delegate Task<IResponse?> HandlerDelegate(Request req);

public static partial class Server
{
    /// <summary>
    /// Whether to block all incoming requests with a message (intended for when the server is updating or shutting down).
    /// </summary>
    public static bool PauseRequests { get; set; } = false;

    /// <summary>
    /// Middleware to attach handlers to ASP.NET.
    /// </summary>
    private class Middleware(RequestDelegate? next)
    {
        private RequestDelegate? Next = next;
        
        /// <summary>
        /// Invoked by ASP.NET for an incoming request with the given HttpContext.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Response.Headers.Append("server", Config.ServerHeader);
                
                if (PauseRequests)
                {
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    context.Response.StatusCode = 503;
                    context.Response.Headers.RetryAfter = "10";
                    await context.Response.WriteAsync("The server is not accepting requests at this time, most likely because it is being updated. Please try again in a few seconds.");
                    return;
                }
                
                var req = new Request(context);
                using var response = await GetResponse(req, Next, context);
                await response.Respond(req, context);
            } catch { }
        }
    
        internal static async Task<IResponse> GetResponse(Request req, RequestDelegate? next, HttpContext? context)
        {
            try
            {
                foreach (HandlerDelegate layer in Config.HandlingLayers)
                {
                    var response = await layer.Invoke(req);
                    if (response != null)
                        return response;
                }
            
                if (Config.AllowMoreMiddlewaresIfUnhandled && next != null)
                {
                    if (context != null)
                        await next.Invoke(context);
                    return new DummyResponse();
                }
                else
                    return StatusResponse.NotImplemented;
            }
            catch (DatabaseEntryMissingException)
            {
                return StatusResponse.NotFound;
            }
            catch (ForcedResponse forcedResponse)
            {
                return forcedResponse.Response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException at '{req.ProtoHostPath}':\n{ex.Message}\n{ex.StackTrace}\n");
                return new ExceptionResponse(ex);
            }
        }
    }
    
    public static Task<IResponse> GetOtherResponseAsync(Request req, string url)
        => Middleware.GetResponse(new(req, url), null, null);
    
    /// <summary>
    /// Adds the correct timestamp to the given URL if it points to this server.
    /// </summary>
    public static async Task<string> ResourcePath(Request req, string url)
    {
        var otherRequest = new Request(req, url);
        using var response = await Middleware.GetResponse(otherRequest, null, null);
        return response is AbstractFileResponse { Timestamp: not null } fileResponse
            ? url + Parsers.QueryStringSuffix(otherRequest.Query.FullString, $"t={fileResponse.Timestamp}")
            : url;
    }
}