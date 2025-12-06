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
public class Request(LayerRequestData data)
{
    #region Properties

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
    private readonly HttpContext Context = data.Context;

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
    private User? _User = data.User;

    /// <summary>
    /// The associated user table.
    /// </summary>
    private UserTable? _UserTable = data.UserTable;

    /// <summary>
    /// The current login state.
    /// </summary>
    public LoginState LoginState { get; internal set; } = data.LoginState;

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
    
    public string ProtoHostPathQuery
        => data.Context.ProtoHostPathQuery();
    
    public string ProtoHostPath
        => data.Context.ProtoHostPath();
    
    public string ProtoHost
        => data.Context.ProtoHost();
    
    public string Proto
        => data.Context.Proto();
    
    public string? IP
        => data.Context.IP();
    
    public bool IsHttps
        => Context.Request.IsHttps;
    
    public string QueryString
        => Context.Request.QueryString.Value ?? "";

    /// <summary>
    /// The associated user. If no user is associated with the request, an exception is thrown.<br/>
    /// A user is only associated if LoginState is not None or Banned. This can also be checked by getting bool IRequest.HasUser.
    /// </summary>
    public User User
    {
        get => _User ?? throw new Exception("This request doesn't contain a user.");
        internal set => _User = value;
    }

    /// <summary>
    /// Whether a user is associated with the request.
    /// </summary>
    public bool HasUser
        => _User != null;

    /// <summary>
    /// The associated user table. If no table is assigned to requests to this domain, an exception is thrown.
    /// </summary>
    public UserTable UserTable
    {
        get => _UserTable ?? throw new Exception("This request isn't referencing a user table.");
        internal set => _UserTable = value;
    }

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
    public int Status => Context.Response.StatusCode;

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
    
    /// <summary>
    /// Returns the URL of the requested page's origin.
    /// </summary>
    public string? CanonicalUrl
        => Server.Config.Domains.CanonicalDomains.TryGetValueAny(out var domain, Domains) ? $"{Context.Proto()}{domain}{Context.PathQuery()}" : null;

    #endregion

    #region Basic methods

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

    #region Interface

    /// <summary>
    /// Returns a query string (including '?') with the current 'redirect' parameter or an empty string if no such parameter was provided.
    /// </summary>
    public string CurrentRedirectQuery
        => Query.TryGetValue("redirect", out var redirect) ? ("?redirect=" + HttpUtility.UrlEncode(redirect)) : "";

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
}