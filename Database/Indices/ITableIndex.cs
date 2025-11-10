namespace uwap.Database;

/// <summary>
/// Contains functionality to efficiently find table entries.
/// </summary>
public interface ITableIndex<in T> where T : AbstractTableValue
{
    /// <summary>
    /// Updates the index so the given ID is attached to the given value (null if it was deleted).
    /// </summary>
    public void Update(string id, T? value);
}