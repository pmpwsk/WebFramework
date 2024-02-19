using System.Collections.ObjectModel;

namespace uwap.WebFramework;

/// <summary>
/// Information about a part of a backup to restore.
/// </summary>
public class RestorePartInfo
{
    /// <summary>
    /// The name of this backup part.
    /// </summary>
    public readonly string PartName;

    /// <summary>
    /// The IDs of the backups to restore from (ordered from the fresh backup to the last backup in the chain).
    /// </summary>
    public readonly ReadOnlyCollection<string> BackupIds;

    /// <summary>
    /// The directory tree for the IDs of the backups each file should be taken from.
    /// </summary>
    public readonly BackupTree OriginIdTree;

    /// <summary>
    /// Creates a new object with information about a part of a backup to restore.
    /// </summary>
    public RestorePartInfo(string partName, ReadOnlyCollection<string> ids)
    {
        PartName = partName;

        BackupIds = ids;

        OriginIdTree = new();
    }
}