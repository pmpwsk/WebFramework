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
    /// Creates a new object for a script or style with the given URL.
    /// </summary>
    public ScriptOrStyle(string url)
    {
        if (!url.EndsWith(Extension))
            throw new ArgumentException("The URL must end with " + Extension);
        Url = url;
    }

    /// <summary>
    /// Enumerates the script or style element's lines.
    /// </summary>
    public IEnumerable<string> Export(IRequest request)
    {
        var domains = Parsers.Domains(request.Domain);
        var entry = FindEntry(domains);
        if (entry == null)
        {
            IPlugin? plugin = PluginManager.GetPlugin(domains, Url, out string relPath, out _);
            if (plugin != null)
            {
                string? timestamp = plugin.GetFileVersion(relPath);
                if (timestamp != null)
                    yield return BuildReference(Url + "?t=" + timestamp);
            }
            yield return BuildReference(Url);
        }
        else if (entry.IsPublic)
            yield return BuildReference(Url + "?t=" + entry.GetModifiedUtc().Ticks);
        else
        {
            yield return $"<{Tag}>";
            foreach (string line in entry.EnumerateTextLines())
                yield return "\t" + line;
            yield return $"</{Tag}>";
        }
    }

    /// <summary>
    /// Finds the cache entry for the script or style, or returns null if it shouldn't be found.
    /// </summary>
    /// <param name="domains">List of accepted domains.</param>
    private Server.CacheEntry? FindEntry(List<string> domains)
    {
        Server.CacheEntry? entry;
        if (Url.StartsWith("http"))
        {
            string? u = Url.Remove(0, 4);
            if (u.StartsWith("://")) u = u.Remove(0, 3);
            else if (u.StartsWith("s://")) u.Remove(0, 4);
            else u = null;

            if (u != null)
            {
                int firstSlash = u.IndexOf('/');
                if (firstSlash != -1)
                {
                    string afterSlash = u.Remove(0, firstSlash);
                    if (afterSlash.StartsWith('/')) afterSlash = afterSlash.Remove(0, 1);
                    foreach (string domain in domains)
                        if (Server.Cache.TryGetValue(domain + "/" + afterSlash, out entry)) return entry;
                    if (Server.Cache.TryGetValue(afterSlash, out entry)) return entry;
                }
            }
        }
        else if (Url.StartsWith('/'))
        {
            foreach (string domain in domains)
                if (Server.Cache.TryGetValue(domain + Url, out entry)) return entry;
        }
        else if (!Url.Contains('/'))
        {
            if (Server.Cache.TryGetValue(Url, out entry)) return entry;
        }
        return null;
    }
}