using uwap.WebFramework.Plugins;
using uwap.WebFramework.Responses;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Calls plugin or event handlers.
        /// </summary>
        public static async Task<IResponse?> HandlerLayer(Request req)
        {
            switch (req.Method)
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
                    IPlugin? plugin = PluginManager.GetPlugin(req, req.Domains, req.Path, out string relPath, out string pathPrefix, out _);

                    //file
                    if (req.Method == "GET" && plugin != null)
                    {
                        byte[]? file = plugin.GetFile(relPath, pathPrefix, req.Domain);
                        string? timestamp = plugin.GetFileVersion(relPath);
                        if (file != null && timestamp != null)
                            return new ByteArrayResponse(file, Parsers.Extension(relPath), true, timestamp);
                    }
                    
                    //handler
                    if (req.Method == "GET")
                    {
                        var response = ParsePage(req, req.Domains) ?? MarkdownParser.HandleRequest(req, req.Domains);
                        if (response != null)
                            return response;
                    }
                    
                    if (plugin != null)
                    {
                        if (relPath == "")
                            return new RedirectResponse(req.FullPath + '/');
                        
                        req.Path = relPath;
                        req.PluginPathPrefix = pathPrefix;
                        return await CallPluginHandlerAsync(plugin, req);
                    }
                    
                    if (OtherRequestHandler != null)
                        return await OtherRequestHandler(req);
                    
                    if (Config.AllowMoreMiddlewaresIfUnhandled)
                        return null;
                    
                    return new DummyResponse();
                
                default: //method unknown
                    return StatusResponse.BadMethod;
            }
        }
    
        /// <summary>
        /// Sets the plugin as a dependency for the asynchronous context and calls the plugin's handler.
        /// </summary>
        private static async Task<IResponse> CallPluginHandlerAsync(IPlugin plugin, Request req)
        {
            Dependencies.Register(plugin);
            return await plugin.HandleAsync(req);
        }
    }
}