using uwap.WebFramework.Responses;
using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        private const string DynamicPageLayerPrefix = "/wf/dyn";
        
        public static async Task<IResponse?> DynamicPageLayer(Request req)
        {
            if (!req.Path.StartsWith(DynamicPageLayerPrefix + '/'))
                return null;
            
            var path = req.Path[DynamicPageLayerPrefix.Length..];
            
            switch (path)
            {
                case "/watcher":
                {
                    req.ForceGET();
                    var url = req.Query.GetOrThrow("url");
                    
                    var otherResponse = await GetOtherResponseAsync(req, url);
                    if (otherResponse is not Page page)
                        return StatusResponse.NotFound;
                    var watcher = WatcherManager.CreateWatcher(page);
                    
                    var response = new EventResponse();
                    await response.KeepEventAliveCancelled.RegisterAsync((_, _) =>
                    {
                        watcher.EventResponse = null;
                        WatcherManager.DeleteWatcher(watcher);
                        return Task.CompletedTask;
                    });
                    watcher.EventResponse = response;
                    response.OnStart = () =>
                    {
                        watcher.WritePage(page);
                        return Task.CompletedTask;
                    };
                    return response;
                }
                
                default:
                    return StatusResponse.NotFound;
            }
        }
    }
}