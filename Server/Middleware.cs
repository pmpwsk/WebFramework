using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Ocsp;
using System.Web;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Elements;
using uwap.WebFramework.Plugins;
using static uwap.WebFramework.Server.Config;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// Whether to block all incoming requests with a message (intended for when the server is updating or shutting down).
    /// </summary>
    public static bool PauseRequests = false;

    /// <summary>
    /// Calls the handler for a given API request (used by AppRequest.CallApi).
    /// </summary>
    internal static async Task CallApi(ApiRequest request)
    {
        var domains = Parsers.Domains(request.Domain);
        string path = request.Path;
        if (path.StartsWith("/api"))
            path = path.Remove(0, 4);
        if (await PluginManager.Handle(domains, path, request)) { }
        else if (ApiRequestReceived != null) await ApiRequestReceived.Invoke(request);
        else request.Status = 501;
    }

    /// <summary>
    /// Calls the handler for a given download request (used by AppRequest.CallDownload).
    /// </summary>
    internal static async Task CallDownload(DownloadRequest request)
    {
        var domains = Parsers.Domains(request.Domain);
        string path = request.Path;
        if (path.StartsWith("/dl")) 
            path = path.Remove(0, 3);
        if (await PluginManager.Handle(domains, path, request)) { }
        else if (DownloadRequestReceived != null) await DownloadRequestReceived.Invoke(request);
        else request.Status = 501;
    }

    /// <summary>
    /// Middleware to attach handlers to ASP.NET.
    /// </summary>
    private class Middleware
    {
        /// <summary>
        /// Required constructor for ASP.NET.
        /// </summary>
        public Middleware(RequestDelegate next) { }

        /// <summary>
        /// Invoked by ASP.NET for an incoming request with the given HttpContext.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Response.Headers.Add("server", Config.ServerHeader);
                context.Response.StatusCode = 200;

                if (PauseRequests)
                {
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    await context.Response.WriteAsync("The server is not accepting requests at this time, most likely because it is being updated. Please try again in a few seconds.");
                    return;
                }

                string domain = context.Request.Host.Host;
                string path = context.Request.Path;
                
                //reply with acme challenge from automatic certificates
                if (AutoCertificate.Email != null && context.Request.Path.ToString().StartsWith("/.well-known/acme-challenge/"))
                {
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    ApiRequest request = new(context, null, null, LoginState.None);
                    string url = request.Domain + request.Path;
                    if (AutoCertificateTokens.TryGetValue(url, out string? value))
                        await request.Write(value);
                    else request.Status = 404;
                    await request.Finish();
                    return;
                }

                //redirect to https if enabled
                if (Config.HttpsPort != null && !context.Request.IsHttps)
                {
                    context.Response.Redirect($"https://{domain}{(Config.HttpsPort==443 ? "" : $":{Config.HttpsPort}")}{path}{context.Request.QueryString}", true);
                    return;
                }

                if (Config.Domains.Redirect.TryGetValue(context.Domain(), out string? redirectTarget))
                {
                    context.Response.Redirect(context.Proto() + redirectTarget + context.Path() + context.Query(), true);
                    return;
                }

                User? user;
                UserTable? userTable = AccountManager.Settings.Enabled ? AccountManager.GetUserTable(context) : null;
                LoginState loginState;

                if (userTable != null)
                    loginState = userTable.Authenticate(context, out user);
                else
                {
                    user = null;
                    loginState = LoginState.None;
                }

                //get available domains
                var domains = Parsers.Domains(domain);

                //handle the rest based on method
                switch (context.Request.Method.ToUpper())
                {
                    case "GET": //get method
                    {
                        //serve files directly, if available
                        foreach (string d in domains)
                        {
                            if (await context.ServeFile($"{d}{path}")) return;
                            if (await context.ServeFile($"{d}{path}.html")) return;
                        }

                        if (path.StartsWith("/api/") || AccountManager.Settings.LoginAllowedPaths == null || AccountManager.Settings.LoginAllowedPaths.Contains(path)) { }
                        else if (loginState == LoginState.NeedsMailVerification)
                        {
                            if (path != AccountManager.Settings.MailVerifyPath)
                            {
                                IPlugin? plugin = PluginManager.GetPlugin(domains, context.Path(), out string relPath, out string pathPrefix);
                                if (plugin == null || plugin.GetFileVersion(relPath) == null)
                                {
                                    context.Response.Redirect($"{AccountManager.Settings.MailVerifyPath}?redirect={HttpUtility.UrlEncode(context.PathQuery())}");
                                    break;
                                }
                            }
                        }
                        else if (loginState == LoginState.Needs2FA)
                        {
                            if (path != AccountManager.Settings.TwoFactorPath)
                            {
                                IPlugin? plugin = PluginManager.GetPlugin(domains, context.Path(), out string relPath, out string pathPrefix);
                                if (plugin == null || plugin.GetFileVersion(relPath) == null)
                                {
                                    context.Response.Redirect($"{AccountManager.Settings.TwoFactorPath}?redirect={HttpUtility.UrlEncode(context.PathQuery())}");
                                    break;
                                }
                            }
                        }

                        context.Response.Headers.Add("Cache-Control", "no-cache, private");

                        //handle based on the first segment
                        string prefix = context.Request.Path.Value??"/";
                        if (prefix.Length <= 1) prefix = "";
                        else
                        {
                            prefix = prefix.Remove(0, 1);
                            if (prefix.Contains('/')) prefix = prefix.Remove(prefix.IndexOf('/'));
                            else prefix = "";
                        }
                        switch (prefix)
                        {
                            case "api": //api request
                            {
                                context.Response.ContentType = "text/plain;charset=utf-8";
                                ApiRequest request = new(context, user, userTable, loginState);
                                try
                                {
                                    string path2 = request.Path;
                                    if (path2.StartsWith("/api")) 
                                        path2 = path2.Remove(0, 4);
                                    if (await PluginManager.Handle(domains, path2, request)) { }
                                    else if (ApiRequestReceived != null) await ApiRequestReceived.Invoke(request);
                                    else request.Status = 501;
                                }
                                catch (Exception ex)
                                {
                                    request.Exception = ex;
                                    request.Status = 500;
                                }
                                try { await request.Finish(); } catch { }
                            } break;
                            case "dl": //download request
                            {
                                DownloadRequest request = new(context, user, userTable, loginState);
                                try
                                {
                                    string path2 = request.Path;
                                    if (path2.StartsWith("/dl")) 
                                        path2 = path2.Remove(0, 3);
                                    if (await PluginManager.Handle(domains, path2, request)) { }
                                    else if (DownloadRequestReceived != null) await DownloadRequestReceived.Invoke(request);
                                    else request.Status = 501;
                                }
                                catch (Exception ex)
                                {
                                    request.Exception = ex;
                                    request.Status = 500;
                                }
                                try { await request.Finish(); } catch { }
                            } break;
                            default: //app request
                            {
                                IPlugin? plugin = PluginManager.GetPlugin(domains, context.Path(), out string relPath, out string pathPrefix);
                                if (plugin != null)
                                {
                                    byte[]? file = plugin.GetFile(relPath, pathPrefix, context.Domain());
                                    string? timestamp = plugin.GetFileVersion(relPath);
                                    if (file != null && timestamp != null)
                                    {
                                        //headers
                                        if (context.AddFileHeaders(Parsers.Extension(relPath), timestamp))
                                            break;

                                        //send file
                                        ApiRequest fileRequest = new(context, null, null, LoginState.None);
                                        fileRequest.CorsDomain = Config.FileCorsDomain;
                                        try
                                        {
                                            await fileRequest.SendBytes(file);
                                        }
                                        catch (Exception ex)
                                        {
                                            fileRequest.Exception = ex;
                                            fileRequest.Status = 500;
                                            try { await fileRequest.Finish(); } catch { }
                                        }
                                        break;
                                    }
                                }

                                context.Response.ContentType = "text/html;charset=utf-8";
                                AppRequest request = new(context, user, userTable, loginState);
                                try
                                {
                                    if (ParsePage(request, domains)) { }
                                    else
                                    {
                                        string path2 = request.Path;
                                        if (path2 == "/") path2 = "";
                                        if (plugin != null) await plugin.Handle(request, relPath, pathPrefix);
                                        else if (AppRequestReceived != null) await AppRequestReceived.Invoke(request);
                                        else request.Status = 501;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    request.Exception = ex;
                                    request.Status = 500;
                                }
                                try { await request.Finish(); } catch { }
                            } break;
                        }
                    } break;
                    case "POST": //post method
                    {
                        context.Response.Headers.Add("Cache-Control", "no-cache, private");
                        context.Response.ContentType = "text/plain;charset=utf-8";
                        if ((!context.Request.HasFormContentType) || context.Request.Form.Files.Count == 0)
                        { //regular post
                            PostRequest request = new(context, user, userTable, loginState);
                            try
                            {
                                string path2 = request.Path;
                                if (path2 == "/") path2 = "";
                                if (await PluginManager.Handle(domains, path2, request)) { }
                                else if (PostRequestReceived != null) await PostRequestReceived.Invoke(request);
                                else request.Status = 501;
                            }
                            catch (Exception ex)
                            {
                                request.Exception = ex;
                                request.Status = 500;
                            }
                            try { await request.Finish(); } catch { }
                        }
                        else
                        { //file upload
                            UploadRequest request = new(context, user, userTable, loginState);
                            try
                            {
                                string path2 = request.Path;
                                if (path2 == "/") path2 = "";
                                if (await PluginManager.Handle(domains, path2, request)) { }
                                else if (UploadRequestReceived != null) await UploadRequestReceived.Invoke(request);
                                else request.Status = 501;
                            }
                            catch (Exception ex)
                            {
                                request.Exception = ex;
                                request.Status = 500;
                            }
                            try { await request.Finish(); } catch { }
                        }
                    } break;
                    case "HEAD": //just the headers should be returned
                        context.Response.Headers.Add("Cache-Control", "no-cache, private");
                        if (context.Path() != "/")
                            context.Response.StatusCode = 404; //currently, only path / is "handled" to allow for web server discovery using HEAD requests
                        break;
                    default: //method not supported
                    {
                        context.Response.Headers.Add("Cache-Control", "no-cache, private");
                        context.Response.StatusCode = 405;
                        await context.Response.WriteAsync(Parsers.StatusMessage(405));
                    } break;
                }
            } catch { }
        }
    }

    /// <summary>
    /// Attempts to serve a requested file and returns true, or returns false if no matching file was found.
    /// </summary>
    private static async Task<bool> ServeFile(this HttpContext context, string key)
    {
        try
        {
            if ((!Cache.ContainsKey(key))
                || (!Cache[key].IsPublic)
                || (context.Request.Path.Value ?? "/").EndsWith(".html")
                || (context.Request.Path.Value ?? "/").EndsWith(".wfpg")
                || key.Contains(".."))
                return false;

            CacheEntry entry = Cache[key];

            //don't handle if the file isn't present in the cache and no longer exists
            if (entry.File == null && !File.Exists($"../Public/{key}")) return false;

            //headers
            if (context.AddFileHeaders(entry.Extension, entry.GetModifiedUtc().Ticks.ToString()))
                return true;

            //send file
            ApiRequest request = new(context, null, null, LoginState.None);
            request.CorsDomain = Config.FileCorsDomain;
            try
            {
                if (entry.File == null) await request.SendFile($"../Public/{key}");
                else await request.SendBytes(entry.File.Content);
            }
            catch (Exception ex)
            {
                request.Exception = ex;
                request.Status = 500;
                try { await request.Finish(); } catch { }
            }
        }
        catch (Exception ex)
        {
            ApiRequest request = new(context, null, null, LoginState.None);
            request.Exception = ex;
            request.Status = 500;
            await request.Finish();
        }

        return true;
    }

    /// <summary>
    /// Adds the headers for file serving (type, cache). If the browser already has the latest version (=abort), true is returned, otherwise false.
    /// </summary>
    private static bool AddFileHeaders(this HttpContext context, string extension, string timestamp)
    {
        //content type
        if (MimeTypes.TryGetValue(extension, out string? type)) context.Response.ContentType = type;

        //browser cache
        if (BrowserCacheMaxAge.TryGetValue(extension, out int maxAge))
        {
            if (maxAge == 0) context.Response.Headers.Add("Cache-Control", "no-cache, private");
            else
            {
                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                    context.Response.Headers.Add("Cache-Control", "public, max-age=" + maxAge);
                else context.Response.Headers["Cache-Control"] = "public, max-age=" + maxAge;
                try
                {
                    if (context.Request.Headers.TryGetValue("If-None-Match", out var oldTag) && oldTag == timestamp)
                    {
                        context.Response.StatusCode = 304;
                        if (Config.FileCorsDomain != null) context.Response.Headers.Add("Access-Control-Allow-Origin", Config.FileCorsDomain);
                        return true; //browser already has the current version
                    }
                    else context.Response.Headers.Add("ETag", timestamp);
                }
                catch { }
            }
        }
        return false;
    }
}