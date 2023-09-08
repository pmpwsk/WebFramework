namespace uwap.Database;

/// <summary>
/// Interface for tables to allow unified error checking (and fixing) and key listing.
/// </summary>
public interface ITable
{
    /// <summary>
    /// Checks the table for errors and attempts to fix them.
    /// </summary>
    public void CheckAndFix();

    /// <summary>
    /// Lists all keys that are present in the table.
    /// </summary>
    public List<string> ListKeys();
}