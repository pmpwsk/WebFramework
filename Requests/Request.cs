using System.Security.Cryptography.X509Certificates;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Elements;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework;

/// <summary>
/// Unified class for all possible requests.
/// </summary>
public class Request(LayerRequestData data)
{
    #region Properties

    /// <summary>
    /// The current state of the request.
    /// </summary>
    public RequestState State {get; private set;} = RequestState.Open;

    /// <summary>
    /// The page object to be written or null if no page is associated with the request.
    /// </summary>
    public IPage? Page = null;

    /// <summary>
    /// The relative path for a plugin's request or the full path for other requests.
    /// The path segments are already URL path decoded, except for %2f (slash).
    /// </summary>
    public string Path {get; internal set;} = data.Path;

    /// <summary>
    /// The handling plugin's path prefix or an empty string for other requests.
    /// </summary>
    public string PluginPathPrefix {get; internal set;} = "";

    /// <summary>
    /// The associated HttpContext object.
    /// </summary>
    public readonly HttpContext Context = data.Context;

    /// <summary>
    /// The associated cookie manager.
    /// </summary>
    public readonly CookieManager Cookies = data.Cookies;

    /// <summary>
    /// The associated query manager.
    /// The query values are already URL decoded.
    /// </summary>
    public readonly QueryManager Query = data.Query;

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    private readonly User? _User = data.User;

    /// <summary>
    /// The associated user table.
    /// </summary>
    private readonly UserTable? _UserTable = data.UserTable;

    /// <summary>
    /// The current login state.
    /// </summary>
    public readonly LoginState LoginState = data.LoginState;

    /// <summary>
    /// The exception that occurred or null if no exception interrupted the request handling.
    /// </summary>
    internal Exception? Exception = null;

    /// <summary>
    /// The list of domains associated with this request.
    /// </summary>
    public List<string> Domains = data.Domains;

    #endregion

    #region Request data

    /// <summary>
    /// The HTTP method.
    /// </summary>
    public string Method
        => data.Context.Request.Method.ToUpper();

    /// <summary>
    /// The associated user. If no user is associated with the request, an exception is thrown.<br/>
    /// A user is only associated if LoginState is not None or Banned. This can also be checked by getting bool IRequest.HasUser.
    /// </summary>
    public User User
        => _User ?? throw new Exception("This request doesn't contain a user.");

    /// <summary>
    /// Whether a user is associated with the request.
    /// </summary>
    public bool HasUser
        => _User != null;

    /// <summary>
    /// The associated user table. If no table is assigned to requests to this domain, an exception is thrown.
    /// </summary>
    public UserTable UserTable
        => _UserTable ?? throw new Exception("This request isn't referencing a user table.");

    /// <summary>
    /// Whether a user table is assigned to requests to this domain.
    /// </summary>
    public bool HasUserTable
        => _UserTable != null;

    /// <summary>
    /// Whether the user is fully logged in.
    /// </summary>
    public bool LoggedIn
        => LoginState == LoginState.LoggedIn;

    /// <summary>
    /// Whether the user is an administrator (access level = ushort.MaxValue). Also returns false if the client isn't fully logged in.
    /// </summary>
    public bool IsAdmin
        => LoginState == LoginState.LoggedIn && User.AccessLevel == ushort.MaxValue;

    /// <summary>
    /// The requested path.
    /// </summary>
    public string FullPath
        => Context.Request.Path.Value ?? "/";

    /// <summary>
    /// The requested domain.
    /// </summary>
    public string Domain
        => Context.Domain();

    /// <summary>
    /// The response status to be sent.
    /// </summary>
    public int Status
    {
        get => Context.Response.StatusCode;
        set => Context.Response.StatusCode = value;
    }

    /// <summary>
    /// The URL that is specified in the 'redirect' parameter, or "/" if no such parameter has been provided.
    /// Not allowed (returns "/"): URLs that don't start with /, https:// or http://.
    /// </summary>
    public string RedirectUrl
        => Query.TryGetValue("redirect", out var url) && url.StartsWithAny("/", "https://", "http://") ? url : "/";

    /// <summary>
    /// Returns the client certificate for the request, either from the initial connection or by requesting it from the client (<c>Server.Config.EnableDelayedClientCertificates</c> needs to be <c>true</c> for second option).<br/>
    /// If no client certificate was located, null is returned.
    /// </summary>
    public async Task<X509Certificate2?> GetClientCertificate() => Context.Connection.ClientCertificate ?? await Context.Connection.GetClientCertificateAsync();

    #endregion

    #region Basic methods

    /// <summary>
    /// Writes the page/text/status and marks the request as finished.
    /// </summary>
    public async Task Finish()
    {
        switch (State)
        {
            case RequestState.Finished:
                return;
            case RequestState.Event:
                break;
            case RequestState.Text:
                if (!WriteTextImmediately)
                    if (Status == 500 && Exception != null && (IsAdmin || Server.DebugMode))
                    {
                        Context.Response.ContentType = "text/plain;charset=utf-8";
                        await Context.Response.WriteAsync($"{Exception.GetType().FullName??"Exception"}\n{Exception.Message}\n{Exception.StackTrace??"No stacktrace"}");
                    }
                    else
                    {
                        Context.Response.ContentType ??= "text/plain;charset=utf-8";
                        await Context.Response.WriteAsync(TextBuffer);
                    }
                break;
            case RequestState.Open:
                if (Page != null)
                {
                    Context.Response.ContentType = "text/html;charset=utf-8";
                    foreach (string line in Page.Export(this))
                        await Context.Response.WriteAsync(line + "\n");
                }
                else
                {
                    Context.Response.ContentType ??= "text/plain;charset=utf-8";
                    await Context.Response.WriteAsync((Status == 500 && Exception != null && (IsAdmin || Server.DebugMode))
                        ? $"{Exception.GetType().FullName??"Exception"}\n{Exception.Message}\n{Exception.StackTrace??"No stacktrace"}"
                        : Parsers.StatusMessage(Status));
                }
                break;
        }

        State = RequestState.Finished;
    }

    /// <summary>
    /// Redirects the client to the given URL. 'permanent' (default: false) indicates whether the page has been moved permanently or just temporarily.
    /// </summary>
    public void Redirect(string url, bool permanent = false)
        => Context.Response.Redirect(url, permanent);

    /// <summary>
    /// Marks the request as finished so the Finish() method won't be called.
    /// </summary>
    public void MarkAsFinished()
    => State = RequestState.Finished;

    /// <summary>
    /// Throws a BadMethodSignal (status 405) if the HTTP method is something other than GET.
    /// </summary>
    public void ForceGET()
    {
        if (Method != "GET")
            throw new BadMethodSignal();
    }

    /// <summary>
    /// Throws a BadMethodSignal (status 405) if the HTTP method is something other than POST.
    /// </summary>
    public void ForcePOST()
    {
        if (Method != "POST")
            throw new BadMethodSignal();
    }

    /// <summary>
    /// Throws a RedirectToLoginSignal if LoggedIn is false.
    /// </summary>
    public void ForceLogin(bool redirect = true)
    {
        if (!LoggedIn)
            if (redirect)
                throw new RedirectToLoginSignal(this);
            else throw new ForbiddenSignal();
    }

    /// <summary>
    /// Throws a ForbiddenSignal if IsAdmin is false.
    /// </summary>
    public void ForceAdmin(bool redirectIfNotLoggedIn = true)
    {
        if (LoggedIn)
        {
            if (!IsAdmin)
                throw new ForbiddenSignal();
        }
        else if (redirectIfNotLoggedIn)
            throw new RedirectToLoginSignal(this);
        else throw new ForbiddenSignal();
    }

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets Page to the new page and returns it.
    /// </summary>
    public Page CreatePage(string title)
    {
        return Presets.CreatePage(this, title);
    }
    
    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets Page to the new page and returns as an out parameter.
    /// </summary>
    public void CreatePage(string title, out Page page)
    {
        page = Presets.CreatePage(this, title);
    }

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets Page to the new page and returns it and its list of elements as an out parameter for easy access.
    /// </summary>
    public void CreatePage(string title, out Page page, out List<IPageElement> e)
    {
        page = Presets.CreatePage(this, title);
        e = page.Elements;
    }

    #endregion

    #region Interface

    /// <summary>
    /// Redirects the user to the set login path with the current path as a parameter (key: redirect).
    /// </summary>
    public void RedirectToLogin()
    {
        State = RequestState.Finished;
        Redirect(Presets.LoginPath(this) + "?redirect=" + HttpUtility.UrlEncode(Context.ProtoHostPathQuery()));
    }

    /// <summary>
    /// Returns a query string (including '?') with the current 'redirect' parameter or an empty string if no such parameter was provided.
    /// </summary>
    public string CurrentRedirectQuery
        => Query.TryGetValue("redirect", out var redirect) ? ("?redirect=" + HttpUtility.UrlEncode(redirect)) : "";

    #endregion

    #region Non-interface

    /// <summary>
    /// The only origin domain the data gotten from the response should be used for (or null to disable).
    /// </summary>
    public string? CorsDomain
    {
        set
        {
            if (value != null)
                Context.Response.Headers.Append("Access-Control-Allow-Origin", value);
        }
    }

    #endregion

    #region Text

    /// <summary>
    /// Text response buffer.
    /// Default: empty string
    /// </summary>
    private string TextBuffer = "";

    /// <summary>
    /// Whether to write text immediately (true) or to the buffer (false, gets written when the request is finished).
    /// </summary>
    private bool _WriteTextImmediately = false;

    /// <summary>
    /// Whether to write text immediately (true) or to the buffer (false, gets written when the request is finished).
    /// </summary>
    public bool WriteTextImmediately
    {
        get => _WriteTextImmediately;
        set => _WriteTextImmediately = State == RequestState.Open ? value : throw new Exception("Something has already been written/sent.");
    }

    /// <summary>
    /// Writes the given text either directly or to the buffer.
    /// </summary>
    public async Task Write(string text)
    {
        switch (State)
        {
            case RequestState.Open:
                if (WriteTextImmediately)
                {
                    Context.Response.ContentType ??= "text/plain;charset=utf-8";
                    await Context.Response.WriteAsync(text);
                }
                else TextBuffer += text;
                State = RequestState.Text;
                break;
            case RequestState.Text:
                if (WriteTextImmediately)
                    await Context.Response.WriteAsync(text);
                else TextBuffer += text;
                break;
            case RequestState.Event:
                throw new Exception("The request is in event mode.");
            case RequestState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }
    /// <summary>
    /// Writes the given text and a new line either directly or to the buffer.
    /// </summary>
    public async Task WriteLine(string text)
        => await Write(text + "\n");

    #endregion

    #region File/bytes

    /// <summary>
    /// Sends the file at the given path as a response.
    /// </summary>
    public async Task WriteFile(string path)
    {
        switch (State)
        {
            case RequestState.Open:
                string extension = new FileInfo(path).Extension;
                if (Server.Config.MimeTypes.TryGetValue(extension, out string? type))
                    Context.Response.ContentType = type;
                Context.Response.ContentLength = new FileInfo(path).Length;
                await Context.Response.SendFileAsync(path);
                State = RequestState.Finished;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                throw new Exception("The request is in event mode.");
            case RequestState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }

    /// <summary>
    /// Sends the given byte array as a response. If a known file extension is provided, its assigned MIME type will be sent.
    /// </summary>
    public async Task WriteBytes(byte[] bytes, string? extension = null)
    {
        switch (State)
        {
            case RequestState.Open:
                if (extension != null && Server.Config.MimeTypes.TryGetValue(extension, out string? type))
                    Context.Response.ContentType = type;
                Context.Response.ContentLength = bytes.Length;
                await Context.Response.BodyWriter.WriteAsync(bytes);
                State = RequestState.Finished;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                throw new Exception("The request is in event mode.");
            case RequestState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }

    /// <summary>
    /// Sends the file at the given path as a download with the given file name. If a file name is provided, that replaces the actual file name.
    /// </summary>
    public async Task WriteFileAsDownload(string path, string? filename = null)
    {
        switch (State)
        {
            case RequestState.Open:
                filename ??= new FileInfo(path).Name;
                if (filename.Contains('.'))
                {
                    string extension = filename.Remove(0, filename.LastIndexOf('.'));
                    if (Server.Config.MimeTypes.TryGetValue(extension, out string? type))
                        Context.Response.ContentType = type;
                }
                Context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
                Context.Response.ContentLength = new FileInfo(path).Length;
                await Context.Response.SendFileAsync(path);
                State = RequestState.Finished;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                throw new Exception("The request is in event mode.");
            case RequestState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }

    /// <summary>
    /// Sends the given byte array as a download with the given file name.
    /// </summary>
    public async Task WriteBytesAsDownload(byte[] bytes, string filename)
    {
        switch (State)
        {
            case RequestState.Open:
                if (filename.Contains('.'))
                {
                    string extension = filename.Remove(0, filename.LastIndexOf('.'));
                    if (Server.Config.MimeTypes.TryGetValue(extension, out string? type))
                        Context.Response.ContentType = type;
                }
                Context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
                Context.Response.ContentLength = bytes.Length;
                await Context.Response.BodyWriter.WriteAsync(bytes);
                State = RequestState.Finished;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                throw new Exception("The request is in event mode.");
            case RequestState.Finished:
                throw new Exception("The request has already been completed.");
        }
    }

    #endregion

    #region POST/forms

    /// <summary>
    /// The largest allowed request body size for this request in bytes. This may only be set once and only before any reading has begun.
    /// </summary>
    public long? BodySizeLimit
    {
        set => (Context.Features.Get<IHttpMaxRequestBodySizeFeature>() ?? throw new Exception("IHttpMaxRequestBodySizeFeature is not supported.")).MaxRequestBodySize = value;
    }

    /// <summary>
    /// Whether the request has set a content type for a form.
    /// </summary>
    public bool IsForm
        => Context.Request.HasFormContentType;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form
        => Context.Request.Form;

    /// <summary>
    /// The uploaded files.
    /// </summary>
    public IFormFileCollection Files
        => Context.Request.Form.Files;

    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public async Task<string> GetBodyText()
    {
        using StreamReader reader = new(Context.Request.Body, true);
        try
        {
            return await reader.ReadToEndAsync();
        }
        finally
        {
            reader.Close();
        }
    }

    /// <summary>
    /// The request body, interpreted as bytes.
    /// </summary>
    public async Task<byte[]> GetBodyBytes()
    {
        using MemoryStream target = new();
        await Context.Request.Body.CopyToAsync(target);
        return target.ToArray();
    }

    #endregion

    #region Event

    /// <summary>
    /// Lock that assures that only one thread at a time can send an event message.
    /// </summary>
    private readonly AsyncLock EventLock = new();

    /// <summary>
    /// Sends the given message to the client as an event.
    /// </summary>
    public async Task EventMessage(string message)
    {
        switch (State)
        {
            case RequestState.Open:
                Context.Response.ContentType = "text/event-stream";
                State = RequestState.Event;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                break;
            case RequestState.Finished:
                throw new Exception("The request has already been finished.");
        }

        using var h = await EventLock.WaitAsync();
        
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        await Context.Response.WriteAsync($"data: {message}\r\r", cts.Token);
        await Context.Response.Body.FlushAsync(cts.Token);
    }

    /// <summary>
    /// Sends ":keepalive" every 30 seconds as long as the given cancellation token (if present) hasn't been cancelled.
    /// </summary>
    public async Task KeepEventAlive(CancellationToken cancellationToken = default)
    {
        switch (State)
        {
            case RequestState.Open:
                Context.Response.ContentType = "text/event-stream";
                State = RequestState.Event;
                break;
            case RequestState.Text:
                throw new Exception("The request is in text mode.");
            case RequestState.Event:
                break;
            case RequestState.Finished:
                throw new Exception("The request has already been finished.");
        }

        using CancellationTokenSource cts = new();
        Context.RequestAborted.Register(cts.Cancel);
        Server.StoppingToken.Register(cts.Cancel);
        if (cancellationToken != CancellationToken.None)
            cancellationToken.Register(cts.Cancel);
            
        try
        {
            while (!cts.IsCancellationRequested)
            {
                await EventMessage(":keepalive");
                await Task.Delay(30000, cts.Token);
            }
        }
        catch { }

        await KeepEventAliveCancelled.InvokeWithAsyncCaller
        (
            s => s(this),
            _ => {},
            true
        );
    }

    /// <summary>
    /// The function to call when KeepEventAlive was canceled (because the client has disconnected, the server is shutting down or the provided token was cancelled).<br/>
    /// Default: null
    /// </summary>
    public readonly SubscriberContainer<Func<Request, Task>> KeepEventAliveCancelled = new();

    #endregion
}