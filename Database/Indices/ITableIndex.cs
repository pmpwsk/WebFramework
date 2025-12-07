namespace uwap.WebFramework.Database;

/// <summary>
/// Contains functionality to efficiently find table entries.
/// </summary>
public interface ITableIndex<in T> : IDisposable where T : AbstractTableValue
{
    /// <summary>
    /// Updates the index so the given ID is attached to the given value (null if it was deleted).
    /// </summary>
    public Task UpdateAsync(string id, T? value);
}