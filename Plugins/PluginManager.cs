using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Manages plugins and handles requests for them.
/// </summary>
public static class PluginManager
{
    /// <summary>
    /// The structure that stores all of the plugins.
    /// </summary>
    private static PluginMap Plugins = new();

    /// <summary>
    /// Finds the plugin that most closely matches the given path for any of the given domains and returns it and the relative path that is left over (rest), or returns null if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static IPlugin? GetPlugin(IEnumerable<string> domains, string path, out string relPath, out string pathPrefix)
    {
        Dictionary<int, IPlugin> results = new();
        foreach (var domain in domains)
        {
            Plugins.GetPlugins(results, 0, (domain + path).Split('/'));
        }
        if (!results.Any())
        {
            relPath = "";
            pathPrefix = "";
            return null;
        }
        int max = results.Keys.Max();
        relPath = "/" + string.Join('/', path.Split('/').Skip(max));
        pathPrefix = path.Remove(path.Length - relPath.Length + 1);
        if (pathPrefix.EndsWith("/"))
            pathPrefix = pathPrefix.Substring(0, pathPrefix.Length - 1);
        if (path == "/" || (relPath == "/" && !path.EndsWith("/")))
            relPath = "";
        return results[max];
    }

    /*This is no longer used as it has been moved to the middleware in order to allow file serving!
    /// <summary>
    /// Handles the given page request with the plugin that most closely matches the given path for any of the given domains, or does nothing and returns false if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static async Task<bool> Handle(List<string> domains, string path, AppRequest req)
    {
        var plugin = GetPlugin(domains, path, out string relPath, out string pathPrefix);
        if (plugin == null) return false;
        await plugin.Handle(req, relPath, pathPrefix);
        return true;
    }*/

    /// <summary>
    /// Handles the given API request with the plugin that most closely matches the given path for any of the given domains, or does nothing and returns false if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static async Task<bool> Handle(IEnumerable<string> domains, string path, ApiRequest req)
    {
        var plugin = GetPlugin(domains, path, out string relPath, out string pathPrefix);
        if (plugin == null) return false;
        await plugin.Handle(req, relPath, pathPrefix);
        return true;
    }

    /// <summary>
    /// Handles the given download request with the plugin that most closely matches the given path for any of the given domains, or does nothing and returns false if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static async Task<bool> Handle(IEnumerable<string> domains, string path, DownloadRequest req)
    {
        var plugin = GetPlugin(domains, path, out string relPath, out string pathPrefix);
        if (plugin == null) return false;
        await plugin.Handle(req, relPath, pathPrefix);
        return true;
    }

    /// <summary>
    /// Handles the given POST request (without any files) with the plugin that most closely matches the given path for any of the given domains, or does nothing and returns false if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static async Task<bool> Handle(IEnumerable<string> domains, string path, PostRequest req)
    {
        var plugin = GetPlugin(domains, path, out string relPath, out string pathPrefix);
        if (plugin == null) return false;
        await plugin.Handle(req, relPath, pathPrefix);
        return true;
    }

    /// <summary>
    /// Handles the given upload request with the plugin that most closely matches the given path for any of the given domains, or does nothing and returns false if no matching plugin was found.<br/>
    /// Domains should be sorted by their priority among plugins with the same depth (the most relevant domain should be first).
    /// </summary>
    public static async Task<bool> Handle(IEnumerable<string> domains, string path, UploadRequest req)
    {
        var plugin = GetPlugin(domains, path, out string relPath, out string pathPrefix);
        if (plugin == null) return false;
        await plugin.Handle(req, relPath, pathPrefix);
        return true;
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
        Dictionary<IPlugin, string> plugins = new();
        foreach (var p in Plugins.Children)
        {
            AddPlugins(plugins, p.Key, p.Value);
        }

        foreach (var p in plugins)
        {
            try
            {
                await p.Key.Work();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling the worker method for plugin at '{p.Value}': {ex.Message}");
            }
        }

    }

    /// <summary>
    /// Recursively adds the plugins (key) and their URLs (value) from the given plugin map to the given dictionary.
    /// </summary>
    private static void AddPlugins(Dictionary<IPlugin,string> plugins, string name, PluginMap map)
    {
        if (map.Plugin != null)
        {
            if (!plugins.ContainsKey(map.Plugin))
                plugins[map.Plugin] = name;
        }

        foreach (var p in map.Children)
        {
            AddPlugins(plugins, $"{name}/{p.Key}", p.Value);
        }
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
