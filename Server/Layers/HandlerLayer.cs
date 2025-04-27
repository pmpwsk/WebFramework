﻿using Microsoft.AspNetCore.Http;
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
                case "GET":
                case "HEAD":
                case "POST":
                case "PUT":
                case "DELETE":
                case "CONNECT":
                case "OPTIONS":
                case "TRACE":
                case "PATCH":
                    data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");

                    IPlugin? plugin = PluginManager.GetPlugin(data.Context, data.Domains, data.Path, out string relPath, out string pathPrefix, out _);

                    //file
                    if (data.Method == "GET" && plugin != null)
                    {
                        byte[]? file = plugin.GetFile(relPath, pathPrefix, data.Domain);
                        string? timestamp = plugin.GetFileVersion(relPath);
                        if (file != null && timestamp != null)
                        {
                            //headers
                            if (AddFileHeaders(data.Context, Parsers.Extension(relPath), timestamp))
                                return true;

                            //send file
                            Request fileRequest = new(data) { CorsDomain = Config.FileCorsDomain };
                            try
                            {
                                await fileRequest.WriteBytes(file);
                            }
                            catch (Exception ex)
                            {
                                fileRequest.Exception = ex;
                                fileRequest.Status = 500;
                                try { await fileRequest.Finish(); } catch { }
                            }
                        }
                    }
                    
                    //handler
                    Request req = new(data);
                    try
                    {
                        if (data.Method == "GET" && ParsePage(req, data.Domains))
                        {
                        }
                        else if (plugin != null)
                        {
                            if (relPath == "")
                                req.Redirect(req.FullPath + '/');
                            else
                            {
                                req.Path = relPath;
                                req.PluginPathPrefix = pathPrefix;
                                await plugin.Handle(req);
                            }
                        }
                        else if (RequestReceived != null)
                            await RequestReceived.Invoke(req);
                        else return false;
                    }
                    catch (RedirectSignal redirect)
                    {
                        try { req.Redirect(redirect.Location, redirect.Permanent); } catch { }
                    }
                    catch (HttpStatusSignal status)
                    {
                        try { req.Status = status.Status; } catch { }
                    }
                    catch (Exception ex)
                    {
                        req.Exception = ex;
                        req.Status = 500;
                    }
                    try { await req.Finish(); } catch { }
                    break;
                default: //method unknown
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
}