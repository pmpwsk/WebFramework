using uwap.WebFramework.Database;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework;

/// <summary>
/// Delegate for a layer of the middleware, returns whether the request has been finished (the middleware will not continue if true was returned).
/// </summary>
public delegate Task<bool> LayerDelegate(LayerRequestData data);

public delegate Task<bool> HandlerDelegate(Request req);

public static partial class Server
{
    /// <summary>
    /// Adds the correct timestamp to the given URL if it points to this server.
    /// </summary>
    public static string ResourcePath(Request req, string url)
    {
        Parsers.FormatPath(req.Context, url, req.Domains, out var path, out var domains, out var query);
        var timestamp = ResourceTimestamp(req, path, domains, query);
        return timestamp != null
            ? url + Parsers.QueryStringSuffix(query, $"t={timestamp}")
            : url;
    }
        
    private static string? ResourceTimestamp(Request req, string path, List<string> domains, string? query)
    {
        foreach (var layer in Config.Layers)
            if (layer == Layers.FileLayer)
            {
                if (Cache.TryGetValueAny(out var entry, domains.Select(d => d + path).ToArray()) && entry.IsPublic)
                    return entry.GetModifiedUtc().Ticks.ToString();
            }
            else if (layer == Layers.SystemFilesLayer)
            {
                if (!path.StartsWith(Layers.SystemFilesLayerPrefix + '/'))
                    continue;
                var relPath = path[Layers.SystemFilesLayerPrefix.Length..];
                return SystemFiles.GetFileVersion(relPath);
            }
            else if (layer == Layers.HandlerLayer)
            {
                IPlugin? plugin = PluginManager.GetPlugin(req.Context, domains, path, out string relPath, out string pathPrefix, out string domain);
                return plugin?.GetFileVersion(relPath);
            }
            
        return null;
    }
    
    public static partial class Layers
    {
        
        public static async Task<bool> Handle(LayerRequestData data, HandlerDelegate handler)
        {
            Request req = new(data);
            try
            {
                try
                {
                    if (!await handler(req))
                        return false;
                }
                catch (DatabaseEntryMissingException)
                {
                    throw new NotFoundSignal();
                }
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
                Console.WriteLine($"\nException at '{req.Context.ProtoHostPathQuery()}':\n{ex.Message}\n{ex.StackTrace}\n");
                req.Exception = ex;
                try { req.Status = 500; } catch { }
            }
            try { await req.Finish(); } catch { }
            
            return true;
        }
    }
}