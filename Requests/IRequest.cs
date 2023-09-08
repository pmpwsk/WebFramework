using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Abstract class for all possible requests.
/// </summary>
public abstract class IRequest
{
    /// <summary>
    /// The associated HttpContext object.
    /// </summary>
    public readonly HttpContext Context;

    /// <summary>
    /// The associated cookie manager.
    /// </summary>
    public readonly CookieManager Cookies;

    /// <summary>
    /// The associated query manager.
    /// </summary>
    public readonly QueryManager Query;

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    public readonly User? User;

    /// <summary>
    /// The associated user table.
    /// </summary>
    private readonly UserTable? _UserTable = null;

    /// <summary>
    /// The current login state.
    /// </summary>
    public readonly LoginState LoginState;

    /// <summary>
    /// The exception that occurred or null if no exception interrupted the request handling.
    /// </summary>
    internal Exception? Exception = null;

    /// <summary>
    /// Creates a new base object for a request object with the given context, user, user table and login state.
    /// </summary>
    public IRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState)
    {
        Context = context;
        User = user;
        _UserTable = userTable;
        LoginState = loginState;
        Cookies = new CookieManager(context);
        Query = new QueryManager(context.Request.Query);
    }

    /// <summary>
    /// The associated user table. If no table is assigned to requests to this domain, an exception is thrown.
    /// </summary>
    public UserTable UserTable => _UserTable ?? throw new Exception("This request isn't referencing a user table.");

    /// <summary>
    /// Whether a user table is assigned to requests to this domain.
    /// </summary>
    public bool HasUserTable => _UserTable != null;

    /// <summary>
    /// The requested path.
    /// </summary>
    public string Path
    {
        get
        {
            if (Context.Request.Path.Value == null) return "/";
            else return Context.Request.Path.Value;
        }
    }

    /// <summary>
    /// The requested domain.
    /// </summary>
    public string Domain => Context.Request.Host.Host;

    /// <summary>
    /// The response status to be sent.
    /// </summary>
    public int Status
    {
        get => Context.Response.StatusCode;
        set => Context.Response.StatusCode = value;
    }

    /// <summary>
    /// Redirects the client to the given URL. 'permanent' (default: false) indicates whether the page has been moved permanently or just temporarily.
    /// </summary>
    public void Redirect(string url, bool permanent = false) => Context.Response.Redirect(url, permanent);

    /// <summary>
    /// The URL that is specified in the 'redirect' parameter, or "/" if no such parameter has been provided.
    /// Not allowed (returns "/"): URLs that are not domain-internal (not starting with '/'), URLs starting with "/api/"
    /// </summary>
    public string RedirectUrl
    {
        get
        {
            string? url = Query.TryGet("redirect");
            if (url == null || (!url.StartsWith("/")) || url.StartsWith("/api/"))
                return "/";
            else return url;
        }
    }

    /// <summary>
    /// Whether the user is fully logged in.
    /// </summary>
    public bool LoggedIn
    {
        get => LoginState == LoginState.LoggedIn;
    }

    /// <summary>
    /// Whether the user is an administrator (access level = ushort.MaxValue). Also returns false if the client isn't fully logged in.
    /// </summary>
    public bool IsAdmin()
        => LoginState == LoginState.LoggedIn && User != null && User.AccessLevel == ushort.MaxValue;

    /// <summary>
    /// Writes a default message for the current response code with MIME type text/plain.
    /// If an exception is saved to the request and the client is fully logged in as an administrator, its message and stack trace are written instead.
    /// </summary>
    internal async Task WriteStatus()
    {
        Context.Response.ContentType = "text/plain";
        if (Status == 500 && Exception != null && IsAdmin())
            await Context.Response.WriteAsync($"{Exception.GetType().FullName??"Exception"}\n{Exception.Message}\n{Exception.StackTrace??"No stacktrace"}");
        else await Context.Response.WriteAsync(Parsers.StatusMessage(Status));
    }
}