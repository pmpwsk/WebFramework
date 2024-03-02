using Microsoft.AspNetCore.Http;
using System.Web;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework;

/// <summary>
/// Intended for frontend requests (pages).
/// </summary>
public class AppRequest(LayerRequestData data) : IRequest(data)
{
    /// <summary>
    /// Whether the page has been written yet.
    /// </summary>
    private bool Finished = false;

    /// <summary>
    /// The page object to be written.
    /// </summary>
    public IPage? Page = null;

    /// <summary>
    /// Writes the page and marks the request as finished. If no page was set, a status message for code 501 (not implemented) is written.
    /// </summary>
    public async Task Finish()
    {
        if (Finished)
            throw new Exception("The page has already been written.");

        Finished = true;
        if (Page == null && Status != 200)
        {
            Presets.CreatePage(this, Status==301||Status==302 ? "Redirecting" : "Error", out Page page, out _);
            Presets.Navigation(this, page);
        }
        if (Page != null)
            foreach (string line in Page.Export(this))
                await Context.Response.WriteAsync(line + "\n");
        else
        {
            Status = 501;
            await Context.Response.WriteAsync(Parsers.StatusMessage(501));
        }
    }

    /// <summary>
    /// Calls the API request handler and marks this request as finished because it may not be used anymore.
    /// </summary>
    public async Task CallApi()
    {
        if (Finished)
            throw new Exception("The page has already been written.");

        Finished = true;
        Context.Response.ContentType = "text/plain;charset=utf-8";
        ApiRequest request = new(new(Context) { User = _User, UserTable = UserTable, LoginState = LoginState, Domains = Domains });
        try
        {
            await Server.CallApi(request);
        }
        catch (Exception ex)
        {
            request.Exception = ex;
            request.Status = 500;
        }
        try { await request.Finish(); } catch { }
    }

    /// <summary>
    /// Calls the download request handler and marks this request as finished because it may not be used anymore.
    /// </summary>
    public async Task CallDownload()
    {
        if (Finished)
            throw new Exception("The page has already been written.");

        Finished = true;
        Context.Response.ContentType = null;
        DownloadRequest request = new(new(Context) { User = _User, UserTable = UserTable, LoginState = LoginState, Domains = Domains });
        try
        {
            await Server.CallDownload(request);
        }
        catch (Exception ex)
        {
            request.Exception = ex;
            request.Status = 500;
        }
        try { await request.Finish(); } catch { }
    }

    /// <summary>
    /// Redirects the user to the set login path with the current path as a parameter (key: redirect).
    /// </summary>
    public void RedirectToLogin()
    {
        Finished = true;
        Redirect(Server.Config.Accounts.LoginPath + "?redirect=" + HttpUtility.UrlEncode(Context.PathQuery()));
    }

    /// <summary>
    /// Returns a query string (including '?') with the current 'redirect' parameter or an empty string if no such parameter was provided.
    /// </summary>
    public string CurrentRedirectQuery()
        => Query.ContainsKey("redirect") ? ("?redirect=" + HttpUtility.UrlEncode(Query["redirect"])) : "";
}