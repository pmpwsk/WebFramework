using System.Collections.ObjectModel;
using System.Text;

namespace uwap.WebFramework;

/// <summary>
/// Information about a part of a backup.
/// </summary>
public class BackupPartInfo
{
    /// <summary>
    /// The name of this backup part.
    /// </summary>
    public readonly string PartName;

    /// <summary>
    /// The current backup's ID.
    /// </summary>
    public readonly string BackupId;

    /// <summary>
    /// The directory tree for the complete last known state (reconstructed from all previous backups in the chain).
    /// </summary>
    public readonly BackupTree LastKnownState;

    /// <summary>
    /// The directory tree for the changes since the previous backup.
    /// </summary>
    public readonly BackupTree Tree;

    /// <summary>
    /// Creates a new object with information about a part of a backup.<br/>
    /// This will create a new folder for the part and load the parts of the same name from previous backups in the chain if there are any.
    /// </summary>
    public BackupPartInfo(string partName, string backupId, ReadOnlyCollection<string> backupBasedOnIds)
    {
        Directory.CreateDirectory($"{Server.Config.Backup.Directory}{backupId}/{partName}");

        PartName = partName;
        BackupId = backupId;

        LastKnownState = new();

        Tree = new();
    }
}