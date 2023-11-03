using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for server-sent event requests to /event/...
/// </summary>
public class EventRequest : IRequest
{
    /// <summary>
    /// Lock that assures that only one thread at a time can send a message.
    /// </summary>
    private ReaderWriterLockSlim Lock;

    /// <summary>
    /// Creates a new event request object with the given context, user, user table and login state.
    /// </summary>
    public EventRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
        Lock = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// Sends the given message to the client as an event.
    /// </summary>
    public async Task Send(string message)
    {
        try
        {
            Lock.EnterWriteLock();
            await Context.Response.WriteAsync($"data: {message}\r\r");
            await Context.Response.Body.FlushAsync();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Sends ":keepalive:" every 30 seconds as long as the given cancallation token (if present) hasn't been cancelled.
    /// </summary>
    public async Task KeepAlive(CancellationToken cancellationToken = default)
    {
        while ((!Context.RequestAborted.IsCancellationRequested) && (cancellationToken == default || !cancellationToken.IsCancellationRequested))
        {
            await Send(":keepalive");
            await Task.Delay(30000, cancellationToken);
        }
    }
}