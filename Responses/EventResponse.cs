using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses;

/// <summary>
/// Sends ":keepalive" every 30 seconds as long as the given cancellation token (if present) hasn't been canceled and allows the sending of messages in between.
/// </summary>
public class EventResponse(CancellationToken cancellationToken = default) : IResponse
{
    public readonly CancellationToken CancellationToken = cancellationToken;
    
    private HttpContext? Context = null;

    /// <summary>
    /// Lock that assures that only one thread at a time can send an event message.
    /// </summary>
    private readonly AsyncLock EventLock = new();
    
    public Func<Task>? OnTick = null;
    
    public Func<Task>? OnStart = null;
    
    public bool ConstantLoop = false;

    /// <summary>
    /// Sends the given message to the client as an event.
    /// </summary>
    public async Task EventMessage(string message)
    {
        if (Context == null)
            throw new Exception("The event hasn't started yet.");
        
        using var h = await EventLock.WaitAsync(CancellationToken);
        
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        await Context.Response.WriteAsync($"data: {message}\r\r", cts.Token);
        await Context.Response.Body.FlushAsync(cts.Token);
    }

    /// <summary>
    /// The event that is called the event has stopped (because the client has disconnected, the server is shutting down or the provided token was canceled).
    /// </summary>
    public readonly SubscriberContainer<Func<Request, EventResponse, Task>> KeepEventAliveCancelled = new();
    
    public async Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.ContentType = "text/event-stream";

        Context = context;
        
        using CancellationTokenSource cts = new();
        Context.RequestAborted.Register(cts.Cancel);
        Server.StoppingToken.Register(cts.Cancel);
        if (CancellationToken != CancellationToken.None)
            CancellationToken.Register(cts.Cancel);
            
        try
        {
            if (OnStart != null)
                await OnStart();
            
            while (!cts.IsCancellationRequested)
            {
                if (!ConstantLoop)
                    await EventMessage(":keepalive");
                
                if (OnTick != null)
                    await OnTick();
                
                if (!ConstantLoop)
                    await Task.Delay(30000, cts.Token);
            }
        }
        catch { }

        await KeepEventAliveCancelled.InvokeWithAsyncCaller
        (
            s => s(req, this),
            _ => {},
            true
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        EventLock.Dispose();
        KeepEventAliveCancelled.Dispose();
    }
}