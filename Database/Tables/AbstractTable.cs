using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the table functionality that doesn't require knowledge of the stored type.
/// </summary>
public abstract class AbstractTable(string name)
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// Returns a dictionary containing each entry's ID and its value's basic value information. 
    /// </summary>
    /// <returns></returns>
    internal abstract Dictionary<string, MinimalTableValue> GetState();
    
    /// <summary>
    /// Returns the version of the stored type.
    /// </summary>
    public abstract Version GetTypeVersion();
    
    /// <summary>
    /// Returns the minimum type version another cluster node needs to communicate with this server.
    /// </summary>
    public abstract Version GetMinVersion();
    
    /// <summary>
    /// Checks and fixes any issues with the table, like memory or disk corruption.
    /// </summary>
    internal abstract void CheckAndFix();
    
    /// <summary>
    /// Lists the nodes that are reachable for this table.
    /// </summary>
    internal abstract ClusterNode[] GetReachableNodes();
    
    /// <summary>
    /// Creates a new locked table entry with the given ID and returns it as an abstract entry.
    /// </summary>
    internal abstract AbstractTableEntry CreateAndLockBlankAbstractEntry(string id);
    
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
    internal abstract void UpdateEntry(ClusterNode node, string id, byte[] serialized);
    
    /// <summary>
    /// Ingests updates from the given node which has the given table state.
    /// </summary>
    internal abstract void SyncFrom(ClusterNode node, Dictionary<string, MinimalTableValue> state);
    
    /// <summary>
    /// Deletes the entry with the given ID and returns whether such an entry existed in the first place.
    /// </summary>
    public abstract bool Delete(string id);
}