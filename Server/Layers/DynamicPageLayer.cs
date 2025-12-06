using uwap.WebFramework.Accounts;
using uwap.WebFramework.Responses;
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
                    var id = req.Query.GetOrThrow("id");
                    
                    if (!WatcherManager.TryGetWatcher(id, out var watcher))
                    {
                        AccountManager.ReportFailedAuth(req);
                        return WatcherManager.RejectResponse;
                    }
                    if (watcher.EventResponse != null)
                        return WatcherManager.RejectResponse;
                    
                    watcher.Expiration.Cancel();
                    var response = new EventResponse();
                    await response.KeepEventAliveCancelled.RegisterAsync((_, _) =>
                    {
                        watcher.EventResponse = null;
                        watcher.Expiration.Start();
                        return Task.CompletedTask;
                    });
                    watcher.EventResponse = response;
                    return response;
                }
                
                default:
                    return StatusResponse.NotFound;
            }
        }
    }
}