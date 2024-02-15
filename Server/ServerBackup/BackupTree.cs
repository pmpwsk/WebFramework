using System.Text;

namespace uwap.WebFramework;

/// <summary>
/// Node for a directory tree that contains data about modified and deleted files and directories.
/// </summary>
public class BackupTree()
{
    /// <summary>
    /// The dictionary of subdirectories (keys = names) in this directory along with their tree node or null if they were deleted.
    /// </summary>
    public readonly Dictionary<string, BackupTree?> Directories = [];

    /// <summary>
    /// The dictionary of files (keys = names) in this directory along with their timestamps (values) or null if they were deleted.
    /// </summary>
    public readonly Dictionary<string, string?> Files = [];
}