using System.Diagnostics.CodeAnalysis;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the table functionality that doesn't require knowledge of the stored type.
/// </summary>
public abstract class AbstractTable
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public readonly string Name;
    
    /// <summary>
    /// The cluster nodes the table should be shared with.<br/>
    /// Operations should use <c>GetReachableNodes()</c> instead since it filters out known-disconnected nodes and shuffles the nodes each time.
    /// </summary>
    internal List<ClusterNode> ClusterNodes;

    protected AbstractTable(string name, List<ClusterNode> clusterNodes)
    {
        if (!clusterNodes.All(node => Server.Config.Database.ClusterNodes.Contains(node)))
            throw new Exception("The table's cluster nodes must be present in Server.Config.Database.ClusterNodes.");
        
        Name = name;
        ClusterNodes = clusterNodes;
    }
    
    /// <summary>
    /// Lists the nodes that are reachable for this table.
    /// </summary>
    internal ClusterNode[] GetReachableNodes()
    {
        var nodes = ClusterNodes.Where(node => node.IsReachable).ToArray();
        Random.Shared.Shuffle(nodes);
        return nodes;
    }

    /// <summary>
    /// The iteration of the stored type, used for migration.<br/>
    /// The value cannot be zero.
    /// </summary>
    public abstract ulong TypeIteration { get; }
    
    /// <summary>
    /// The serializer to use.
    /// </summary>
    public abstract AbstractSerializer Serializer { get; }

    /// <summary>
    /// Returns a dictionary containing each entry's ID and its value's basic value information. 
    /// </summary>
    /// <returns></returns>
    internal abstract Dictionary<string, EntryState> GetState();
    
    /// <summary>
    /// Checks and fixes any issues with the table, like memory or disk corruption.
    /// </summary>
    internal abstract Task CheckAndFixAsync();
    
    /// <summary>
    /// Creates a new locked table entry with the given ID and returns it as an abstract entry.
    /// </summary>
    internal abstract Task<(AbstractTableEntry Entry, AsyncReaderWriterLockHolder Locker)> CreateAndLockBlankAbstractEntryAsync(string id);
    
    /// <summary>
    /// Finds the table entry with the given ID as an abstract entry. 
    /// </summary>
    internal abstract bool TryGetAbstractEntry(string id, [MaybeNullWhen(false)] out AbstractTableEntry entry);
    
    /// <summary>
    /// Lists all table entries as abstract entries.
    /// </summary>
    internal abstract List<AbstractTableEntry> ListAbstractEntries();
    
    /// <summary>
    /// Ingests an update from the given node to the given entry ID using the given serialized value.
    /// </summary>
    internal abstract Task UpdateEntryAsync(ClusterNode node, string id, byte[] serialized);
    
    /// <summary>
    /// Ingests updates from the given node which has the given table state.
    /// </summary>
    internal abstract Task SyncFromAsync(ClusterNode node, Dictionary<string, EntryState> state);
    
    /// <summary>
    /// Deletes the entry with the given ID and returns whether such an entry existed in the first place.
    /// </summary>
    public abstract Task<bool> DeleteAsync(string id);
}