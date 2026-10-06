using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses;

public delegate Task SocketMessageHandler(ReadOnlyMemory<byte> data, bool isText);

/// <summary>
/// Represents a WebSocket response.
/// </summary>
public class SocketResponse(CancellationToken cancellationToken = default) : IResponse
{
    /// <summary>
    /// The custom cancellation token that was passed into the constructor.
    /// </summary>
    public readonly CancellationToken CancellationToken = cancellationToken;
    
    /// <summary>
    /// The combined cancellation token source for all reasons to close the connection.
    /// </summary>
    private CancellationTokenSource? InternalCancellation = null;
    
    /// <summary>
    /// The underlying connection.
    /// </summary>
    private WebSocket? Connection = null;
    
    /// <summary>
    /// Whether the client or the server requested the connection to the closed.<br/>
    /// If the value is false, a connection termination will be treated as unexpected.
    /// </summary>
    public bool ClosureExpected { get; private set; } = false;

    /// <summary>
    /// Lock that assures that only one thread at a time can send data.
    /// </summary>
    private readonly AsyncLock SendLock = new();
    
    /// <summary>
    /// The event that is called once the connection has been established.
    /// </summary>
    public readonly SubscriberContainer<AsyncAction> ConnectionOpened = new();
    
    /// <summary>
    /// The event that is called whenever a new complete message was received.
    /// </summary>
    public readonly SubscriberContainer<SocketMessageHandler> MessageReceived = new();
    
    /// <summary>
    /// The event that is called once the connection has been ended for any reason.
    /// </summary>
    public readonly SubscriberContainer<AsyncAction> ConnectionClosed = new();
    
    /// <summary>
    /// A cancellation token that is canceled once the connection is closed for any reason.
    /// </summary>
    public CancellationToken InternalCancellationToken
        => InternalCancellation?.Token ?? throw new Exception("The socket hasn't started yet.");

    /// <summary>
    /// Sends the given text to the client.
    /// </summary>
    public Task SendTextAsync(string text)
        => SendAsync(WebSocketMessageType.Text, Encoding.UTF8.GetBytes(text), true);

    /// <summary>
    /// Sends the given object as JSON to the client.
    /// </summary>
    public Task SendJsonAsync(object obj)
        => SendAsync(WebSocketMessageType.Text, JsonSerializer.SerializeToUtf8Bytes(obj), true);

    /// <summary>
    /// Sends the given binary data to the client.
    /// </summary>
    public Task SendBinaryAsync(ReadOnlyMemory<byte> data)
        => SendAsync(WebSocketMessageType.Binary, data, true);
    
    /// <summary>
    /// Sends the given binary data to the client in segments while needing to indicate the last segment.
    /// </summary>
    public Task SendSegmentedBinaryAsync(ReadOnlyMemory<byte> data, bool isLastSegment)
        => SendAsync(WebSocketMessageType.Binary, data, isLastSegment);
    
    /// <summary>
    /// Sends the given data to the client.
    /// </summary>
    private async Task SendAsync(WebSocketMessageType type, ReadOnlyMemory<byte> data, bool isLastSegment)
    {
        if (Connection == null || InternalCancellation == null)
            throw new Exception("The socket hasn't started yet.");
        if (Connection.State != WebSocketState.Open || InternalCancellation.IsCancellationRequested)
            throw new Exception("The socket has been closed.");
        
        using var h = await SendLock.WaitAsync(InternalCancellationToken);
        
        await Connection.SendAsync(
            data,
            type,
            isLastSegment ? WebSocketMessageFlags.EndOfMessage : WebSocketMessageFlags.None,
            InternalCancellationToken
        );
    }

    /// <summary>
    /// Closes the socket.
    /// </summary>
    public async Task CloseAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? description = null)
    {
        if (Connection == null || InternalCancellation == null
                || Connection.State == WebSocketState.Closed || Connection.State == WebSocketState.Aborted)
            return;
        
        using var h = await SendLock.WaitAsync(InternalCancellationToken);
        
        try
        {
            ClosureExpected = true;
            await Connection.CloseAsync(
                status,
                description,
                InternalCancellationToken
            );
            await InternalCancellation.CancelAsync();
        }
        catch (WebSocketException) { }
    }
    
    public async Task Respond(Request req, HttpContext context)
    {
        // check method
        if (req.WebSocket == null)
        {
            await StatusResponse.BadMethod.Respond(req, context);
            return;
        }
        
        // open socket
        Connection = await req.WebSocket.AcceptWebSocketAsync(
            new WebSocketAcceptContext
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5),
                KeepAliveTimeout = TimeSpan.FromSeconds(10)
            }
        );
        
        // create cancellation token
        InternalCancellation = new();
        context.RequestAborted.Register(InternalCancellation.Cancel);
        Server.StoppingToken.Register(InternalCancellation.Cancel);
        if (CancellationToken != CancellationToken.None)
            CancellationToken.Register(InternalCancellation.Cancel);
        
        try
        {
            // open event
            await ConnectionOpened.InvokeWithAsyncCaller
            (
                s => s(),
                _ => {},
                true
            );
            
            // repeatedly wait for messages until closed
            var segmentBuffer = new byte[1024];
            while (!InternalCancellation.IsCancellationRequested && Connection.State == WebSocketState.Open)
            {
                // repeatedly collect message segments until complete
                using var messageStream = new MemoryStream();
                WebSocketMessageType? lastMessageType = null;
                while (!InternalCancellation.IsCancellationRequested && Connection.State == WebSocketState.Open)
                {
                    var segmentInfo = await Connection.ReceiveAsync(
                        segmentBuffer,
                        InternalCancellationToken
                    );
                    
                    if (segmentInfo.MessageType == WebSocketMessageType.Close)
                    {
                        // closure message
                        lastMessageType = WebSocketMessageType.Close;
                        await CloseAsync(
                            Connection.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                            Connection.CloseStatusDescription ?? "The client closed the connection."
                        );
                        break;
                    }
                    else if (lastMessageType != null && lastMessageType != segmentInfo.MessageType)
                    {
                        // mixed message types
                        lastMessageType = WebSocketMessageType.Close;
                        await CloseAsync(
                            WebSocketCloseStatus.InvalidMessageType,
                            "The message type changed between segments."
                        );
                        break;
                    }
                    else
                    {
                        // normal message segment
                        lastMessageType = segmentInfo.MessageType;
                        if (segmentInfo.Count > 0)
                            await messageStream.WriteAsync(
                                segmentBuffer.AsMemory(0, segmentInfo.Count),
                                InternalCancellationToken
                            );
                        if (segmentInfo.EndOfMessage)
                            break;
                    }
                }
                
                if (lastMessageType is null or WebSocketMessageType.Close)
                    break;
                
                // combine message segments
                var data = messageStream.GetBuffer().AsMemory(0, (int)messageStream.Length);
                var isText = lastMessageType == WebSocketMessageType.Text;
                
                // message event
                await MessageReceived.InvokeWithAsyncCaller(
                    s => s(data, isText),
                    _ => {},
                    true
                );
            }
        }
        catch { } // cancellation exceptions and more
        
        // cancel the token if not already canceled
        await InternalCancellation.CancelAsync();
        
        // close the connection if not already closed
        if (Connection.State != WebSocketState.Closed && Connection.State != WebSocketState.Aborted)
            try
            {
                var closeCts = new CancellationTokenSource();
                closeCts.CancelAfter(1000);
                await Connection.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable,
                    null,
                    closeCts.Token
                );
            }
            catch { }
        
        // close event
        await ConnectionClosed.InvokeWithAsyncCaller
        (
            s => s(),
            _ => {},
            true
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SendLock.Dispose();
        Connection?.Dispose();
        ConnectionClosed.Dispose();
        InternalCancellation?.Dispose();
    }
}