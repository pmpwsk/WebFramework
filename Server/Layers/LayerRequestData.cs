using System.Security.Cryptography.X509Certificates;
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
    /// The associated query manager.<br/>
    /// The query values are already URL decoded.
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
    /// The path segments are already URL path decoded, except for %2f (slash).
    /// </summary>
    public string Path
        => Context.Request.Path.Value ?? "/";

    /// <summary>
    /// The requested domain.
    /// </summary>
    public string Domain
        => Context.Domain();

    /// <summary>
    /// The HTTP method.
    /// </summary>
    public string Method
        => Context.Request.Method.ToUpper();

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
    
    /// <summary>
    /// The client certificate or <c>null</c> if no client certificate was received.<br/>
    /// The request has to have been received on the <c>Server.Config.ClientCertificatePort</c> for this to work, otherwise <c>null</c> is returned in all cases.<br/>
    /// Note that you should dispose of the resulting certificate object on your own.
    /// </summary>
    public X509Certificate2? ClientCertificate => Context.Connection.ClientCertificate;

    /// <summary>
    /// Requests a client certificate from the client after the TLS handshake has already been completed and return the certificate or <c>null</c> if no client certificate was received.<br/>
    /// <c>Server.Config.EnableDelayedClientCertificates</c> needs to be <c>true</c> for this to work, otherwise <c>null</c> is returned in all cases.
    /// </summary>
    public async Task<X509Certificate2?> RequestClientCertificate() => await Context.Connection.GetClientCertificateAsync();
}