using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Contains data about a request and its known state.
/// </summary>
public class LayerRequestData(HttpContext context)
{
    /// <summary>
    /// The associated HttpContext object.
    /// </summary>
    public readonly HttpContext Context = context;

    /// <summary>
    /// The associated cookie manager.
    /// </summary>
    public readonly CookieManager Cookies = new(context);

    /// <summary>
    /// The associated query manager.
    /// </summary>
    public readonly QueryManager Query = new(context.Request.Query);

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    public User? User = null;

    /// <summary>
    /// The associated user table.
    /// </summary>
    public UserTable? UserTable = null;

    /// <summary>
    /// The current login state.
    /// </summary>
    public LoginState LoginState = LoginState.None;

    /// <summary>
    /// A dictionary of custom objects that layers can use to communicate with each other.
    /// </summary>
    public Dictionary<string, object> CustomObjects = [];

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
    /// The list of domains associated with this request.
    /// </summary>
    public List<string> Domains = [context.Domain()];

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
    /// Whether the user is fully logged in.
    /// </summary>
    public bool LoggedIn
        => LoginState == LoginState.LoggedIn;

    /// <summary>
    /// Whether the user is an administrator (access level = ushort.MaxValue). Also returns false if the client isn't fully logged in.
    /// </summary>
    public bool IsAdmin()
        => LoginState == LoginState.LoggedIn && User != null && User.AccessLevel == ushort.MaxValue;
}