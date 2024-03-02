namespace uwap.WebFramework.Plugins;

/// <summary>
/// Recursively stores plugins, every URL segment is a node on the path to a given plugin.
/// </summary>
internal class PluginMap
{
    /// <summary>
    /// The dictionary of nodes that follow the current node (key is the next segment, value is the plugin map for it)-.
    /// </summary>
    public Dictionary<string, PluginMap> Children = [];

    /// <summary>
    /// The plugin that has the URL ending with the segment of this node or null if no plugin has been mapped to this exact URL.
    /// </summary>
    public IPlugin? Plugin = null;

    /// <summary>
    /// Recursively maps the given plugin to the given URL (domain and path, domain 'any' is supported).<br/>
    /// If another plugin is mapped to the same URL, it is replaced.
    /// </summary>
    public void Map(IEnumerable<string> segments, IPlugin plugin)
    {
        if (!segments.Any())
        {
            Plugin = plugin;
            return;
        }
        if (!Children.TryGetValue(segments.First(), out var child))
        {
            child = new PluginMap();
            Children.Add(segments.First(), child);
        }
        child.Map(segments.Skip(1), plugin);
    }

    /// <summary>
    /// Recursively unmaps the plugin with the given URL (and deletes now empty nodes on the path to it), or does nothing if no plugin with that URL exists.
    /// </summary>
    /// <returns>true if the parent (calling this method) should delete this node (indicating that it is empty), otherwise false.</returns>
    public bool Unmap(IEnumerable<string> segments)
    {
        if (!segments.Any())
        {
            Plugin = null;
            if (Children.Count == 0)
                return true;
            else return false;
        }
        else if (Children.TryGetValue(segments.First(), out var child) && child.Unmap(segments.Skip(1)))
            Children.Remove(segments.First());

        return Plugin == null && Children.Count == 0;
    }

    /// <summary>
    /// Recursively adds the plugins (value) that match the given segments from this node onwards to the given dictionary along with their depth (key).
    /// </summary>
    /// <param name="depth">The depth of this node.</param>
    public void GetPlugins(Dictionary<int,Tuple<IPlugin,string>> results, int depth, IEnumerable<string> segments, string domain)
    {
        if (Plugin != null)
            if (!results.ContainsKey(depth))
                results.Add(depth, new(Plugin,domain));
        
        if (segments.Any() && Children.TryGetValue(segments.First(), out var child))
            child.GetPlugins(results, depth+1, segments.Skip(1), domain);
    }
}
