namespace uwap.WebFramework.Database;

/// <summary>
/// A generic database file action to create, change or delete files.
/// </summary>
public interface IFileAction
{
    /// <summary>
    /// Prepares the file action by moving old things to the trash and writing new things to the buffer.
    /// </summary>
    public void Prepare(AbstractTableValue value);
    
    /// <summary>
    /// Commits to the file action by deleting the old things in the trash and moving new things out of the buffer.
    /// </summary>
    public void Commit(AbstractTableValue value, long timestamp);
}