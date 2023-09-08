using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework.Elements;

public abstract class ScriptOrStyle
{
    protected abstract string LinkCode(string url);
    protected abstract string Tag { get; }
    protected abstract string Extension { get; }

    public string Url;

    public ScriptOrStyle(string url)
    {
        if (!url.EndsWith(Extension))
            throw new ArgumentException("The URL must end with " + Extension);
        Url = url;
    }

    public List<string> Export(IRequest request)
    {
        var domains = Parsers.Domains(request.Domain);
        var entry = FindEntry(request, domains);
        if (entry == null)
        {
            IPlugin? plugin = PluginManager.GetPlugin(domains, Url, out string relPath, out _);
            if (plugin != null)
            {
                string? timestamp = plugin.GetFileVersion(relPath);
                if (timestamp != null)
                    return BuildLink(Url + "?t=" + timestamp);
            }
            return BuildLink(Url);
        }
        else if (entry.IsPublic)
            return BuildLink(Url + "?t=" + entry.GetModifiedUtc().Ticks);
        else
        {
            List<string> result = new List<string>();
            result.Add($"<{Tag}>");
            entry.AppendTextLinesTo(result, "\t");
            result.Add($"</{Tag}>");
            return result;
        }
    }

    private Server.CacheEntry? FindEntry(IRequest request, List<string> domains)
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

    private List<string> BuildLink(string url)
        => new List<string>{LinkCode(url)};
}