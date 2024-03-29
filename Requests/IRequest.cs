using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Abstract class for all possible requests.
/// </summary>
public abstract class IRequest(LayerRequestData data)
{
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
    /// </summary>
    public readonly QueryManager Query = data.Query;

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    public readonly User? _User = data.User;

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
    /// The requested path.
    /// </summary>
    public string Path
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
    /// Redirects the client to the given URL. 'permanent' (default: false) indicates whether the page has been moved permanently or just temporarily.
    /// </summary>
    public void Redirect(string url, bool permanent = false)
        => Context.Response.Redirect(url, permanent);

    /// <summary>
    /// The URL that is specified in the 'redirect' parameter, or "/" if no such parameter has been provided.
    /// Not allowed (returns "/"): URLs that are not domain-internal (not starting with '/'), URLs starting with "/api/"
    /// </summary>
    public string RedirectUrl
    {
        get
        {
            string? url = Query.TryGet("redirect");
            if (url == null || url.Contains("/api/"))
                return "/";
            if (url.StartsWith('/'))
                return url;

            string? domain = null;
            if (url.StartsWith("https://"))
                domain = url.Remove(0, 8);
            else if (url.StartsWith("http://"))
                domain = url.Remove(0, 7);

            if (domain == null)
                return "/";
            int slash = domain.IndexOf('/');
            if (slash == -1)
                return "/";
            domain = domain.Remove(slash);
            if (domain == Domain || AccountManager.GetWildcardDomain(domain) == AccountManager.GetWildcardDomain(Domain))
                return url;
            return "/";
        }
    }

    /// <summary>
    /// Whether the user is fully logged in.
    /// </summary>
    public bool LoggedIn
        => LoginState == LoginState.LoggedIn;

    /// <summary>
    /// Whether the user is an administrator (access level = ushort.MaxValue). Also returns false if the client isn't fully logged in.
    /// </summary>
    public bool IsAdmin()
        => LoginState == LoginState.LoggedIn && User.AccessLevel == ushort.MaxValue;

    /// <summary>
    /// Writes a default message for the current response code with MIME type text/plain.
    /// If an exception is saved to the request and the client is fully logged in as an administrator, its message and stack trace are written instead.
    /// </summary>
    internal async Task WriteStatus()
    {
        Context.Response.ContentType = "text/plain";
        await Context.Response.WriteAsync((Status == 500 && Exception != null && IsAdmin())
            ? $"{Exception.GetType().FullName??"Exception"}\n{Exception.Message}\n{Exception.StackTrace??"No stacktrace"}"
            : Parsers.StatusMessage(Status));
    }
}