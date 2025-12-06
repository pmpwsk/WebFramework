using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// Sends a single message as an event which is closed immediately after sending the message.
/// </summary>
public class SingleEventMessageResponse(string message) : IResponse
{
    private readonly string Message = message;
    
    public async Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.ContentType = "text/event-stream";
        
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        await context.Response.WriteAsync($"data: {Message}\r\r", cts.Token);
        await context.Response.Body.FlushAsync(cts.Token);
    }
}