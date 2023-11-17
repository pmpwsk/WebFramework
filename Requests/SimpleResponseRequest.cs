using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Abstract class that adds CORS and text responses to IRequest.
/// </summary>
public abstract class SimpleResponseRequest : IRequest
{
    /// <summary>
    /// The only origin domain the data gotten from the response should be used for (or null to disable).
    /// </summary>
    public string? CorsDomain
    {
        set
        {
            if (value != null) Context.Response.Headers.Append("Access-Control-Allow-Origin", value);
        }
    }

    /// <summary>
    /// Different possible states an API request can be in.
    /// </summary>
    private enum ResponseState
    {
        /// <summary>
        /// Text has been written, only text can be written.
        /// </summary>
        Text,
        /// <summary>
        /// Writing has finished and no more text or datra can be written.
        /// </summary>
        Finished,
        /// <summary>
        /// Nothing has been written yet, any type can be written.
        /// </summary>
        None
    }

    /// <summary>
    /// The state of this request.
    /// Default: None
    /// </summary>
    private ResponseState State = ResponseState.None;

    /// <summary>
    /// Text response buffer.
    /// Default: empty string
    /// </summary>
    private string Buffer = "";

    /// <summary>
    /// Whether to write text immediately (true) or to the buffer (false, gets written when the request is finished).
    /// </summary>
    private bool _WriteImmediately = false;

    /// <summary>
    /// Whether to write text immediately (true) or to the buffer (false, gets written when the request is finished).
    /// </summary>
    public bool WriteImmediately
    {
        get => _WriteImmediately;
        set
        {
            if (State == ResponseState.None) _WriteImmediately = value;
            else throw new Exception("Something has already been written/sent.");
        }
    }

    /// <summary>
    /// Creates a new TextRequest.
    /// </summary>
    public SimpleResponseRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState)
        : base(context, user, userTable, loginState) { }

    /// <summary>
    /// Writes the given text either directly or to the buffer.
    /// </summary>
    public async Task Write(string text)
    {
        switch (State)
        {
            case ResponseState.None:
                await _Write(text);
                State = ResponseState.Text; break;
            case ResponseState.Text:
                await _Write(text);
                break;
            case ResponseState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }
    /// <summary>
    /// Writes the given text either directly or to the buffer without checking the state.
    /// </summary>
    private async Task _Write(string text)
    {
        if (WriteImmediately) await Context.Response.WriteAsync(text);
        else Buffer += text;
    }
    /// <summary>
    /// Writes the given text and a new line either directly or to the buffer.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task WriteLine(string text)
        => await Write(text + "\n");

    /// <summary>
    /// Marks the request as finished. If text is stored in the buffer, it is written. If nothing was written, a default message for the status code is written.
    /// </summary>
    public async Task Finish()
    {
        if (State == ResponseState.Finished) { }
        else if (WriteImmediately)
        {
            if (State == ResponseState.None) //nothing written, write status message
            {
                State = ResponseState.Finished;
                await WriteStatus();
            }
            else throw new Exception("This method can only be used when not writing immediately.");
        }
        else
        {
            State = ResponseState.Finished;

            if (Status == 200 && Buffer != "")
            {
                await Context.Response.WriteAsync(Buffer);
                Buffer = "";
            }
            else await WriteStatus();
        }
    }

    /// <summary>
    /// Sends the file at the given path as a response.
    /// </summary>
    public async Task SendFile(string path)
    {
        switch (State)
        {
            case ResponseState.None:
                string extension = new FileInfo(path).Extension;
                if (Server.Config.MimeTypes.TryGetValue(extension, out string? type)) Context.Response.ContentType = type;
                await Context.Response.SendFileAsync(path);
                State = ResponseState.Finished; break;
            case ResponseState.Text:
                throw new Exception("This request is in text mode.");
            case ResponseState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }

    /// <summary>
    /// Sends the given byte array as a response. If a known file extension is provided, its assigned MIME type will be sent.
    /// </summary>
    public async Task SendBytes(byte[] bytes, string? extension = null)
    {
        switch (State)
        {
            case ResponseState.None:
                if (extension != null && Server.Config.MimeTypes.TryGetValue(extension, out string? type)) Context.Response.ContentType = type;
                await Context.Response.BodyWriter.WriteAsync(bytes);
                State = ResponseState.Finished; break;
            case ResponseState.Text:
                throw new Exception("This request is in text mode.");
            case ResponseState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }
}