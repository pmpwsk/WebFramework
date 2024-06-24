using System.Text;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework.Elements;

/// <summary>
/// Universal class for scripts and styles as they both need the same things (URL and cache timestamp).
/// </summary>
public abstract class ScriptOrStyle
{
    /// <summary>
    /// "script" or "style", depending on what it is.
    /// </summary>
    protected abstract string Tag { get; }

    /// <summary>
    /// ".js" or ".css", depending on what it is.
    /// </summary>
    protected abstract string Extension { get; }

    /// <summary>
    /// Builds an element for the script or style using the referencing URL instead of sending the script or style directly.
    /// </summary>
    protected abstract string BuildReference(string url);

    /// <summary>
    /// The URL of the script or style.
    /// </summary>
    public string Url;

    /// <summary>
    /// Whether to write the script or style's code directing to the page instead of referencing it.
    /// </summary>
    public bool Flatten;

    /// <summary>
    /// Creates a new object for a script or style with the given URL.
    /// </summary>
    public ScriptOrStyle(string url, bool flatten)
    {
        int queryIndex = url.IndexOf('?');
        if (queryIndex == -1)
        {
            if (!url.EndsWith(Extension))
                throw new ArgumentException("The URL must end with " + Extension);
        }
        else if (!url.Remove(queryIndex).EndsWith(Extension))
            throw new ArgumentException("The URL must end with " + Extension);

        Url = url;
        Flatten = flatten;
    }

    /// <summary>
    /// Enumerates the script or style element's lines.
    /// </summary>
    public IEnumerable<string> Export(Request req)
    {
        int queryIndex = Url.IndexOf('?');
        string urlWithoutQuery = queryIndex > -1 ? Url.Remove(queryIndex) : Url;

        var entry = FindEntry(req.Domains, urlWithoutQuery);
        if (entry == null)
        {
            IPlugin? plugin = PluginManager.GetPlugin(req.Context, req.Domains, urlWithoutQuery, out string relPath, out string pathPrefix, out string domain);
            if (plugin != null)
            {
                string? timestamp = plugin.GetFileVersion(relPath);
                if (timestamp != null)
                {
                    if (Flatten)
                    {
                        yield return $"<{Tag}>";
                        foreach (string line in Encoding.UTF8.GetString(plugin.GetFile(relPath, pathPrefix, domain) ?? []).Split('\n').Select(x => x.TrimEnd('\r')))
                            yield return '\t' + line;
                        yield return $"</{Tag}>";
                    }
                    else yield return BuildReference(Url + (queryIndex > -1 ? "&" : "?") + "t=" + timestamp);
                }
                else yield return BuildReference(Url);
            }
            else yield return BuildReference(Url);
        }
        else if (entry.IsPublic && !Flatten)
            yield return BuildReference(Url + (queryIndex > -1 ? "&" : "?") + "t=" + entry.GetModifiedUtc().Ticks);
        else
        {
            yield return $"<{Tag}>";
            foreach (string line in entry.EnumerateTextLines())
                yield return "\t" + line;
            yield return $"</{Tag}>";
        }
    }

    /// <summary>
    /// Finds the cache entry for the script or style, or returns null if it couldn't be found.
    /// </summary>
    /// <param name="domains">List of accepted domains.</param>
    private static Server.CacheEntry? FindEntry(List<string> domains, string urlWithoutQuery)
    {
        Server.CacheEntry? entry;
        if (urlWithoutQuery.StartsWith("http"))
        {
            string? u = urlWithoutQuery.Remove(0, 4);
            u = u.StartsWith("://") ? u.Remove(0, 3) : (u.StartsWith("s://") ? u.Remove(0, 4) : null);

            if (u != null)
            {
                int firstSlash = u.IndexOf('/');
                if (firstSlash != -1)
                {
                    string afterSlash = u.Remove(0, firstSlash);
                    if (afterSlash.StartsWith('/'))
                        afterSlash = afterSlash.Remove(0, 1);
                    foreach (string domain in domains)
                        if (Server.Cache.TryGetValue(domain + "/" + afterSlash, out entry))
                            return entry;
                    if (Server.Cache.TryGetValue(afterSlash, out entry))
                        return entry;
                }
            }
        }
        else if (urlWithoutQuery.StartsWith('/'))
        {
            foreach (string domain in domains)
                if (Server.Cache.TryGetValue(domain + urlWithoutQuery, out entry))
                    return entry;
        }
        else if ((!urlWithoutQuery.Contains('/')) && Server.Cache.TryGetValue(urlWithoutQuery, out entry))
            return entry;
        return null;
    }
}