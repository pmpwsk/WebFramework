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
public class Request
{
    private HttpContext HttpContext;

    /// <summary>
    /// The associated cookie manager.
    /// </summary>
    public readonly CookieManager Cookies;
    
    /// <summary>
    /// The associated cookie writer or null if the request can't modify the client's cookies.
    /// </summary>
    public readonly CookieWriter? CookieWriter;

    /// <summary>
    /// The associated query manager.
    /// The query values are already URL decoded.
    /// </summary>
    public readonly QueryManager Query;
    
    /// <summary>
    /// The associated form manager.
    /// </summary>
    public readonly FormManager? Form;
    
    /// <summary>
    /// The associated request body manager.
    /// </summary>
    public readonly RequestBodyManager? Body;
    
    /// <summary>
    /// The requesting client's IP address, if available.
    /// </summary>
    public string? ClientAddress;

    /// <summary>
    /// The HTTP method.
    /// </summary>
    public string Method;
    
    /// <summary>
    /// Whether the protocol is HTTPS.
    /// </summary>
    public bool IsHttps;
    
    /// <summary>
    /// The protocol prefix including the colon and two slashes.
    /// </summary>
    public string Proto;
    
    /// <summary>
    /// The requested host name including the port.
    /// </summary>
    public string Host;

    /// <summary>
    /// The requested path.
    /// </summary>
    public string FullPath;

    /// <summary>
    /// The current user or null if no user is logged in.
    /// </summary>
    public User? UserNullable;

    /// <summary>
    /// The associated user table or null if no user table is associated with the request.
    /// </summary>
    public UserTable? UserTableNullable;

    /// <summary>
    /// The current login state.
    /// </summary>
    public LoginState LoginState;

    /// <summary>
    /// The relative path for a plugin's request or the full path for other requests.
    /// The path segments are already URL path decoded, except for %2f (slash).
    /// </summary>
    public string Path;

    /// <summary>
    /// The handling plugin's path prefix or an empty string for other requests.
    /// </summary>
    public string PluginPathPrefix;

    /// <summary>
    /// The list of domains associated with this request.
    /// </summary>
    public List<string> Domains;
    
    /// <summary>
    /// Whether the request originates from the program instance itself.
    /// </summary>
    public readonly bool IsInternal;
    
    public Request(HttpContext context)
    {
        HttpContext = context;
        Cookies = new(context);
        CookieWriter = new(Cookies, context);
        Query = new(context.Query());
        Form = HttpContext.Request.HasFormContentType ? new(context) : null;
        Body = HttpContext.Request.HasFormContentType ? null : new(context);
        ClientAddress = context.IP();
        Method = context.Request.Method.ToUpper();
        IsHttps = context.Request.IsHttps;
        Proto = context.Proto();
        Host = context.Host();
        FullPath = context.Path();
        
        UserNullable = null;
        UserTableNullable = null;
        LoginState = LoginState.None;
        
        Path = context.Request.Path.Value ?? "/";
        PluginPathPrefix = "";
        Domains = Parsers.Domains(Domain);
        IsInternal = false;
        
        Exception = null;
    }
    
    public Request(Request req, string url)
    {
        var parts = Parsers.ParseUrl(req, url);
        
        HttpContext = req.HttpContext;
        Cookies = req.Cookies;
        CookieWriter = null;
        Query = new(parts.Query);
        Form = null;
        Body = null;
        ClientAddress = req.ClientAddress;
        Method = "GET";
        IsHttps = parts.Protocol == "https://";
        Proto = parts.Protocol;
        Host = parts.Host;
        FullPath = parts.Path;
        
        UserNullable = null;
        UserTableNullable = null;
        LoginState = LoginState.None;
        
        Path = string.Join('/', FullPath.Split('/').Select(HttpUtility.UrlDecode));
        PluginPathPrefix = "";
        Domains = Parsers.Domains(Domain);
        IsInternal = true;
        
        Exception = null;
    }

    /// <summary>
    /// The requested host name without the port.
    /// </summary>
    public string Domain
        => Host.Before(':');
    
    /// <summary>
    /// The full requested URL.
    /// </summary>
    public string ProtoHostPathQuery
        => Proto + Host + FullPath + Query.FullString;
    
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
        set => UserTableNullable = value;
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
    public async Task<X509Certificate2?> GetClientCertificate()
        => HttpContext.Connection.ClientCertificate ?? await HttpContext.Connection.GetClientCertificateAsync();
    
    /// <summary>
    /// Returns the URL of the requested page's origin.
    /// </summary>
    public string? CanonicalUrl
        => Server.Config.Domains.CanonicalDomains.TryGetValueAny(out var domain, Domains) ? $"{Proto}{domain}{FullPath}{Query.FullString}" : null;
    
    /// <summary>
    /// The largest allowed request body size for this request in bytes. This may only be set once and only before any reading has begun.
    /// </summary>
    public long? BodySizeLimit
    {
        set => (HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() ?? throw new Exception("IHttpMaxRequestBodySizeFeature is not supported.")).MaxRequestBodySize = value;
    }

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
    
    #region Legacy pages
    
    /// <summary>
    /// The exception that occurred or null if no exception interrupted the request handling.
    /// </summary>
    internal Exception? Exception;

    /// <summary>
    /// The response status to be sent.
    /// </summary>
    public int Status
        => HttpContext.Response.StatusCode;
    
    #endregion
}