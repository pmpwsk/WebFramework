using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Manages plugins and handles requests for them.
/// </summary>
public static class PluginManager
{
    /// <summary>
    /// The structure that stores all of the plugins.
    /// </summary>
    private readonly static PluginMap Plugins = new();

    /// <summary>
    /// Finds the plugin that most closely matches the given path for any of the given domains and returns it and the relative path that is left over (rest), or returns null if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static IPlugin? GetPlugin(HttpContext context, IEnumerable<string> domains, string path, out string relPath, out string pathPrefix, out string domain)
    {
        if (path.StartsWith('/'))
        {
        }
        else if (path.StartsWith("http://"))
        {
            string domainFromPath = path[7..].Before('/').Before('?');
            domains = Parsers.Domains(domainFromPath);
            path = path[(7+domainFromPath.Length)..].Before('?');
        }
        else if (path.StartsWith("https://"))
        {
            string domainFromPath = path[8..].Before('/').Before('?');
            domains = Parsers.Domains(domainFromPath);
            path = path[(8+domainFromPath.Length)..].Before('?');
        }
        else
        {
            List<string> segments = [.. context.Path().Split('/')];
            if (segments.Count > 1)
                segments.RemoveAt(segments.Count - 1);
            foreach (string segment in path.Split('/'))
                switch (segment)
                {
                    case ".":
                        continue;
                    case "..":
                        if (segments.Count == 0 || (segments.Count == 1 && segments.First() == ""))
                            segments.Add(segment);
                        else segments.RemoveAt(segments.Count - 1);
                        break;
                    default:
                        segments.Add(segment);
                        break;
                }
            segments[^1] = path;
            path = string.Join('/', segments);
        }
        
        Dictionary<int, Tuple<IPlugin,string>> results = [];
        foreach (var d in domains)
            Plugins.GetPlugins(results, 0, (d + path).Split('/'), d);

        if (results.Count == 0)
        {
            relPath = "";
            pathPrefix = "";
            domain = "";
            return null;
        }

        int max = results.Keys.Max();
        pathPrefix = string.Join('/', path.Split('/').Take(max));
        if (pathPrefix == "/")
            pathPrefix = "";
        relPath = path[pathPrefix.Length..];
        pathPrefix = context.ProtoHost() + pathPrefix;
        var result = results[max];
        domain = result.Item2;
        return result.Item1;
    }

    /// <summary>
    /// Maps the given plugin to the given URL (domain and path, domain 'any' is supported).<br/>
    /// If another plugin is mapped to the same URL, it is replaced.
    /// </summary>
    public static void Map(string url, IPlugin plugin)
        => Plugins.Map(url.Split('/'), plugin);

    /// <summary>
    /// Unmaps the plugin with the given URL or does nothing if no plugin with that URL exists.
    /// </summary>
    public static void Unmap(string url)
        => Plugins.Unmap(url.Split('/'));

    /// <summary>
    /// Calls the worker method for every mapped plugin.<br/>
    /// If a plugin is mapped to multiple URLs, it is only going to be called once (only if it is the exact same object).
    /// </summary>
    public static async Task Work()
    {
        Dictionary<IPlugin, string> plugins = [];
        foreach (var p in Plugins.Children)
            AddPlugins(plugins, p.Key, p.Value);

        foreach (var p in plugins)
            try
            {
                await p.Key.Work();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling the worker method for plugin at '{p.Value}': {ex.Message}");
            }
    }

    /// <summary>
    /// Calls the backup method for every mapped plugin.<br/>
    /// If a plugin is mapped to multiple URLs, it is only going to be called once (only if it is the exact same object).
    /// </summary>
    public static async Task Backup(string id, ReadOnlyCollection<string> basedOnIds)
    {
        Dictionary<IPlugin, string> plugins = [];
        foreach (var p in Plugins.Children)
            AddPlugins(plugins, p.Key, p.Value);

        foreach (var p in plugins)
            try
            {
                await p.Key.Backup(id, basedOnIds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling the backup method for plugin at '{p.Value}': {ex.Message}");
            }
    }

    /// <summary>
    /// Calls the restore method for every mapped plugin.<br/>
    /// If a plugin is mapped to multiple URLs, it is only going to be called once (only if it is the exact same object).
    /// </summary>
    public static async Task Restore(ReadOnlyCollection<string> ids)
    {
        Dictionary<IPlugin, string> plugins = [];
        foreach (var p in Plugins.Children)
            AddPlugins(plugins, p.Key, p.Value);

        foreach (var p in plugins)
            try
            {
                await p.Key.Restore(ids);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling the restore method for plugin at '{p.Value}': {ex.Message}");
            }
    }

    /// <summary>
    /// Recursively adds the plugins (key) and their URLs (value) from the given plugin map to the given dictionary.
    /// </summary>
    private static void AddPlugins(Dictionary<IPlugin,string> plugins, string name, PluginMap map)
    {
        if (map.Plugin != null)
            if (!plugins.ContainsKey(map.Plugin))
                plugins[map.Plugin] = name;

        foreach (var p in map.Children)
            AddPlugins(plugins, $"{name}/{p.Key}", p.Value);
    }

    /// <summary>
    /// Enumerates the domains that plugins have been mapped to (not "any").
    /// </summary>
    public static IEnumerable<string> GetDomains()
    {
        foreach (string domain in Plugins.Children.Keys)
            if (domain != "any")
                yield return domain;
    }
}
