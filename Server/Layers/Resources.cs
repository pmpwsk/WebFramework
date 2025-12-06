using uwap.WebFramework.Database;
using uwap.WebFramework.Plugins;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public delegate Task<IResponse?> HandlerDelegate(Request req);

public static partial class Server
{
    /// <summary>
    /// Adds the correct timestamp to the given URL if it points to this server.
    /// </summary>
    public static string ResourcePath(Request req, string url)
    {
        Parsers.FormatPath(req, url, req.Domains, out var path, out var domains, out var query);
        var timestamp = ResourceTimestamp(req, path, domains, query);
        return timestamp != null
            ? url + Parsers.QueryStringSuffix(query, $"t={timestamp}")
            : url;
    }
        
    private static string? ResourceTimestamp(Request req, string path, List<string> domains, string? query)
    {
        foreach (var layer in Config.HandlingLayers)
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
                IPlugin? plugin = PluginManager.GetPlugin(req, domains, path, out string relPath, out string pathPrefix, out string domain);
                return plugin?.GetFileVersion(relPath);
            }
            
        return null;
    }
}