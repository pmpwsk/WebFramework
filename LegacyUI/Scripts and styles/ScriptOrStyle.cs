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
        if (!Flatten)
        {
            var changedUrl = Server.ResourcePath(req, Url).GetAwaiter().GetResult();
            if (changedUrl != Url)
            {
                yield return BuildReference(changedUrl);
                yield break;
            }
        }
        
        Parsers.FormatPath(req, Url, req.Domains, out var path, out var domains, out var query);
        if (Server.Cache.TryGetValueAny(out var entry, domains.Select(d => d + path).ToArray()))
            if (entry.IsPublic && !Flatten)
                yield return BuildReference(Url + Parsers.QueryStringSuffix(query, $"t={entry.GetModifiedUtc().Ticks}"));
            else
            {
                yield return $"<{Tag}>";
                foreach (string line in entry.EnumerateTextLines())
                    yield return "\t" + line;
                yield return $"</{Tag}>";
            }
        else
        {
            IPlugin? plugin = PluginManager.GetPlugin(req, domains, path, out string relPath, out string pathPrefix, out string domain);
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
                    else yield return BuildReference(Url + Parsers.QueryStringSuffix(query, $"t={timestamp}"));
                }
                else yield return BuildReference(Url);
            }
            else yield return BuildReference(Url);
        }
    }
}