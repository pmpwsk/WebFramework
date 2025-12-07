using System.Web;
using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response that redirects the response.
/// </summary>
public class RedirectResponse(string location, bool permanent = false) : IResponse
{
    public readonly string Location = location;
    
    public readonly bool Permanent = permanent;
    
    public Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.Redirect(Location, Permanent);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A response that redirects the client to the login page.
/// </summary>
public class RedirectToLoginResponse(Request req)
    : RedirectResponse($"{Presets.LoginPath(req)}?redirect={HttpUtility.UrlEncode(req.ProtoHostPathQuery)}");