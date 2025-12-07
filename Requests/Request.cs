using System.Security.Cryptography.X509Certificates;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

/// <summary>
/// Unified class for all possible requests.
/// </summary>
public class Request(HttpContext context)
{
    #region Basic
    
    private HttpContext HttpContext = context;

    /// <summary>
    /// The associated cookie manager.
    /// </summary>
    public readonly CookieManager Cookies = new(context);

    /// <summary>
    /// The associated query manager.
    /// The query values are already URL decoded.
    /// </summary>
    public readonly QueryManager Query = new(context.Request.Query);
    
    /// <summary>
    /// The requesting client's IP address, if available.
    /// </summary>
    public string? ClientAddress = context.IP();

    /// <summary>
    /// The HTTP method.
    /// </summary>
    public string Method = context.Request.Method.ToUpper();
    
    /// <summary>
    /// Whether the protocol is HTTPS.
    /// </summary>
    public bool IsHttps = context.Request.IsHttps;
    
    /// <summary>
    /// The protocol prefix including the colon and two slashes.
    /// </summary>
    public string Proto = context.Proto();
    
    /// <summary>
    /// The requested host name including the port.
    /// </summary>
    public string Host = context.Host();

    /// <summary>
    /// The requested path.
    /// </summary>
    public string FullPath = context.Path();
    
    /// <summary>
    /// The raw query string including the question mark, or an empty string if no query was provided.
    /// </summary>
    public string QueryString = context.Request.QueryString.Value ?? "";
    
    #endregion
    
    
    
    #region Auth

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    public User? UserNullable = null;

    /// <summary>
    /// The associated user table or null if no user table is associated with the request.
    /// </summary>
    public UserTable? UserTableNullable = null;

    /// <summary>
    /// The current login state.
    /// </summary>
    public LoginState LoginState = LoginState.None;

    #endregion
    
    
    
    #region WF-specific

    /// <summary>
    /// The relative path for a plugin's request or the full path for other requests.
    /// The path segments are already URL path decoded, except for %2f (slash).
    /// </summary>
    public string Path {get; internal set;} = context.Request.Path.Value ?? "/";

    /// <summary>
    /// The handling plugin's path prefix or an empty string for other requests.
    /// </summary>
    public string PluginPathPrefix {get; internal set;} = "";

    /// <summary>
    /// The list of domains associated with this request.
    /// </summary>
    public List<string> Domains = Parsers.Domains(context.Domain());

    #endregion

    
    
    #region Accessors

    /// <summary>
    /// The requested host name without the port.
    /// </summary>
    public string Domain => Host.Before(':');
    
    /// <summary>
    /// The full requested URL.
    /// </summary>
    public string ProtoHostPathQuery
        => Proto + Host + FullPath + QueryString;
    
    /// <summary>
    /// The requested URL without the query.
    /// </summary>
    public string ProtoHostPath
        => Proto + Host + FullPath;
    
    /// <summary>
    /// The requested URL without the path and query.
    /// </summary>
    public string ProtoHost
        => Proto + Host;

    /// <summary>
    /// The associated user. If no user is associated with the request, an exception is thrown.<br/>
    /// A user is only associated if LoginState is not None or Banned. This can also be checked by getting bool IRequest.HasUser.
    /// </summary>
    public User User
    {
        get => UserNullable ?? throw new Exception("This request doesn't contain a user.");
        set => UserNullable = value;
    }

    /// <summary>
    /// Whether a user is associated with the request.
    /// </summary>
    public bool HasUser
        => UserNullable != null;

    /// <summary>
    /// The associated user table. If no table is assigned to requests to this domain, an exception is thrown.
    /// </summary>
    public UserTable UserTable
    {
        get => UserTableNullable ?? throw new Exception("This request isn't referencing a user table.");
        internal set => UserTableNullable = value;
    }

    /// <summary>
    /// Whether a user table is assigned to requests to this domain.
    /// </summary>
    public bool HasUserTable
        => UserTableNullable != null;

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
    /// The URL that is specified in the 'redirect' parameter, or "/" if no such parameter has been provided.
    /// Not allowed (returns "/"): URLs that don't start with /, https:// or http://.
    /// </summary>
    public string RedirectUrl
        => Query.TryGetValue("redirect", out var url) && url.StartsWithAny("/", "https://", "http://") ? url : "/";

    /// <summary>
    /// Returns a query string (including '?') with the current 'redirect' parameter or an empty string if no such parameter was provided.
    /// </summary>
    public string CurrentRedirectQuery
        => Query.TryGetValue("redirect", out var redirect) ? ("?redirect=" + HttpUtility.UrlEncode(redirect)) : "";

    /// <summary>
    /// Returns the client certificate for the request, either from the initial connection or by requesting it from the client (<c>Server.Config.EnableDelayedClientCertificates</c> needs to be <c>true</c> for second option).<br/>
    /// If no client certificate was located, null is returned.
    /// </summary>
    public async Task<X509Certificate2?> GetClientCertificate() => HttpContext.Connection.ClientCertificate ?? await HttpContext.Connection.GetClientCertificateAsync();
    
    /// <summary>
    /// Returns the URL of the requested page's origin.
    /// </summary>
    public string? CanonicalUrl
        => Server.Config.Domains.CanonicalDomains.TryGetValueAny(out var domain, Domains) ? $"{Proto}{domain}{Path}{QueryString}" : null;

    #endregion

    
    
    #region Checks

    /// <summary>
    /// Throws a BadMethodSignal (status 405) if the HTTP method is something other than GET.
    /// </summary>
    public void ForceGET()
    {
        if (Method != "GET")
            throw new ForcedResponse(StatusResponse.BadMethod);
    }

    /// <summary>
    /// Throws a BadMethodSignal (status 405) if the HTTP method is something other than POST.
    /// </summary>
    public void ForcePOST()
    {
        if (Method != "POST")
            throw new ForcedResponse(StatusResponse.BadMethod);
    }

    /// <summary>
    /// Throws a RedirectToLoginSignal if LoggedIn is false.
    /// </summary>
    public void ForceLogin(bool redirect = true)
    {
        if (!LoggedIn)
            if (redirect)
                throw new ForcedResponse(new RedirectToLoginResponse(this));
            else throw new ForcedResponse(StatusResponse.Forbidden);
    }

    /// <summary>
    /// Throws a ForbiddenSignal if IsAdmin is false.
    /// </summary>
    public void ForceAdmin(bool redirectIfNotLoggedIn = true)
    {
        if (LoggedIn)
        {
            if (!IsAdmin)
                throw new ForcedResponse(StatusResponse.Forbidden);
        }
        else if (redirectIfNotLoggedIn)
            throw new ForcedResponse(new RedirectToLoginResponse(this));
        else throw new ForcedResponse(StatusResponse.Forbidden);
    }

    #endregion

    

    #region POST/forms

    /// <summary>
    /// The largest allowed request body size for this request in bytes. This may only be set once and only before any reading has begun.
    /// </summary>
    public long? BodySizeLimit
    {
        set => (HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() ?? throw new Exception("IHttpMaxRequestBodySizeFeature is not supported.")).MaxRequestBodySize = value;
    }

    /// <summary>
    /// Whether the request has set a content type for a form.
    /// </summary>
    public bool IsForm
        => HttpContext.Request.HasFormContentType;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form
        => HttpContext.Request.Form;

    /// <summary>
    /// The uploaded files.
    /// </summary>
    public IFormFileCollection Files
        => HttpContext.Request.Form.Files;

    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public async Task<string> GetBodyText()
    {
        using StreamReader reader = new(HttpContext.Request.Body, true);
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
        await HttpContext.Request.Body.CopyToAsync(target);
        return target.ToArray();
    }

    #endregion
    
    
    
    #region Legacy pages
    
    /// <summary>
    /// The exception that occurred or null if no exception interrupted the request handling.
    /// </summary>
    internal Exception? Exception = null;

    /// <summary>
    /// The response status to be sent.
    /// </summary>
    public int Status => HttpContext.Response.StatusCode;
    
    #endregion
}