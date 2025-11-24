using uwap.WebFramework.Accounts;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        private const string DynamicPageLayerPrefix = "/wf/dyn";
        
        public static async Task<bool> DynamicPageLayer(LayerRequestData data)
            => data.Path.StartsWith(DynamicPageLayerPrefix + '/')
               && await Handle(data, DynamicPageLayer);

        private static async Task<bool> DynamicPageLayer(Request req)
        {
            if (!req.Path.StartsWith(DynamicPageLayerPrefix + '/'))
                return false;
            
            var path = req.Path[DynamicPageLayerPrefix.Length..];
            
            switch (path)
            {
                case "/watcher":
                {
                    req.ForceGET();
                    if (!req.Query.TryGetValue("id", out var id))
                        throw new BadRequestSignal();
                    
                    if (!WatcherManager.TryGetWatcher(id, out var watcher))
                    {
                        AccountManager.ReportFailedAuth(req.Context);
                        await WatcherManager.Reject(req);
                        return true;
                    }
                    if (watcher.Request != null)
                    {
                        await WatcherManager.Reject(req);
                        return true;
                    }
                    
                    watcher.Expiration.Cancel();
                    watcher.Request = req;
                    await req.KeepEventAliveCancelled.RegisterAsync(_ =>
                    {
                        watcher.Request = null;
                        watcher.Expiration.Start();
                        return Task.CompletedTask;
                    });
                    await req.KeepEventAlive();
                } break;
                default:
                    throw new NotFoundSignal();
            }
            
            return true;
        }
    }
}