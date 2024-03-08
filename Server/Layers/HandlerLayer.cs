using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Calls plugin or event handlers.
        /// </summary>
        public static async Task<bool> HandlerLayer(LayerRequestData data)
        {
            switch (data.Context.Request.Method.ToUpper())
            {
                case "GET": //get method
                    {
                        data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");

                        //handle based on the first segment
                        string prefix = data.Path;
                        if (prefix.Length <= 1)
                            prefix = "";
                        else
                        {
                            prefix = prefix.Remove(0, 1);
                            prefix = prefix.Contains('/') ? prefix.Remove(prefix.IndexOf('/')) : "";
                        }
                        switch (prefix)
                        {
                            case "api": //api request
                                {
                                    data.Context.Response.ContentType = "text/plain;charset=utf-8";
                                    ApiRequest request = new(data);
                                    try
                                    {
                                        string path2 = request.Path;
                                        if (path2.StartsWith("/api"))
                                            path2 = path2.Remove(0, 4);
                                        if (await PluginManager.Handle(path2, request))
                                        { }
                                        else if (ApiRequestReceived != null)
                                            await ApiRequestReceived.Invoke(request);
                                        else request.Status = 501;
                                    }
                                    catch (Exception ex)
                                    {
                                        request.Exception = ex;
                                        request.Status = 500;
                                    }
                                    try { await request.Finish(); } catch { }
                                }
                                break;
                            case "dl": //download request
                                {
                                    DownloadRequest request = new(data);
                                    try
                                    {
                                        string path2 = request.Path;
                                        if (path2.StartsWith("/dl"))
                                            path2 = path2.Remove(0, 3);
                                        if (await PluginManager.Handle(path2, request))
                                        { }
                                        else if (DownloadRequestReceived != null)
                                            await DownloadRequestReceived.Invoke(request);
                                        else request.Status = 501;
                                    }
                                    catch (Exception ex)
                                    {
                                        request.Exception = ex;
                                        request.Status = 500;
                                    }
                                    try { await request.Finish(); } catch { }
                                }
                                break;
                            case "event": //event request
                                {
                                    data.Context.Response.ContentType = "text/event-stream";
                                    EventRequest request = new(data);
                                    try
                                    {
                                        string path2 = request.Path;
                                        if (path2.StartsWith("/event"))
                                            path2 = path2.Remove(0, 6);
                                        if (await PluginManager.Handle(path2, request))
                                        { }
                                        else if (EventRequestReceived != null)
                                            await EventRequestReceived.Invoke(request);
                                        else request.Status = 501;
                                    }
                                    catch (Exception ex)
                                    {
                                        request.Exception = ex;
                                        request.Status = 500;
                                    }
                                }
                                break;
                            default: //app request
                                {
                                    IPlugin? plugin = PluginManager.GetPlugin(data.Domains, data.Path, out string relPath, out string pathPrefix, out _);
                                    if (plugin != null)
                                    {
                                        byte[]? file = plugin.GetFile(relPath, pathPrefix, data.Domain);
                                        string? timestamp = plugin.GetFileVersion(relPath);
                                        if (file != null && timestamp != null)
                                        {
                                            //headers
                                            if (AddFileHeaders(data.Context, Parsers.Extension(relPath), timestamp))
                                                break;

                                            //send file
                                            ApiRequest fileRequest = new(data) { CorsDomain = Config.FileCorsDomain };
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

                                    data.Context.Response.ContentType = "text/html;charset=utf-8";
                                    AppRequest request = new(data);
                                    try
                                    {
                                        if (ParsePage(request, data.Domains)) { }
                                        else
                                        {
                                            string path2 = request.Path;
                                            if (path2 == "/")
                                                path2 = "";
                                            if (plugin != null)
                                                await plugin.Handle(request, relPath, pathPrefix);
                                            else if (AppRequestReceived != null)
                                                await AppRequestReceived.Invoke(request);
                                            else request.Status = 501;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        request.Exception = ex;
                                        request.Status = 500;
                                    }
                                    try { await request.Finish(); } catch { }
                                }
                                break;
                        }
                    }
                    break;
                case "POST": //post method
                    {
                        data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");
                        data.Context.Response.ContentType = "text/plain;charset=utf-8";
                        if ((!data.Context.Request.HasFormContentType) || data.Context.Request.Form.Files.Count == 0)
                        { //regular post
                            PostRequest request = new(data);
                            try
                            {
                                string path2 = request.Path;
                                if (path2 == "/")
                                    path2 = "";
                                if (await PluginManager.Handle(path2, request))
                                { }
                                else if (PostRequestReceived != null)
                                    await PostRequestReceived.Invoke(request);
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
                            UploadRequest request = new(data);
                            try
                            {
                                string path2 = request.Path;
                                if (path2 == "/")
                                    path2 = "";
                                if (await PluginManager.Handle(path2, request))
                                { }
                                else if (UploadRequestReceived != null)
                                    await UploadRequestReceived.Invoke(request);
                                else request.Status = 501;
                            }
                            catch (Exception ex)
                            {
                                request.Exception = ex;
                                request.Status = 500;
                            }
                            try { await request.Finish(); } catch { }
                        }
                    }
                    break;
                case "HEAD": //just the headers should be returned
                    {
                        data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");
                        if (data.Path != "/")
                            data.Status = 404; //currently, only path / is "handled" to allow for web server discovery using HEAD requests
                    }
                    break;
                default: //method not supported
                    {
                        data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");
                        data.Status = 405;
                        await data.Context.Response.WriteAsync(Parsers.StatusMessage(405));
                    }
                    break;
            }

            return true;
        }
    }

    /// <summary>
    /// Calls the handler for a given API request (used by AppRequest.CallApi).
    /// </summary>
    internal static async Task CallApi(ApiRequest request)
    {
        string path = request.Path;
        if (path.StartsWith("/api"))
            path = path.Remove(0, 4);
        if (await PluginManager.Handle(path, request))
        { }
        else if (ApiRequestReceived != null)
            await ApiRequestReceived.Invoke(request);
        else request.Status = 501;
    }

    /// <summary>
    /// Calls the handler for a given download request (used by AppRequest.CallDownload).
    /// </summary>
    internal static async Task CallDownload(DownloadRequest request)
    {
        string path = request.Path;
        if (path.StartsWith("/dl"))
            path = path.Remove(0, 3);
        if (await PluginManager.Handle(path, request))
        { }
        else if (DownloadRequestReceived != null)
            await DownloadRequestReceived.Invoke(request);
        else request.Status = 501;
    }
}