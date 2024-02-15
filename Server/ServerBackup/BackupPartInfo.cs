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
        foreach (string backupBasedOnId in backupBasedOnIds)
            Load(backupBasedOnId);

        Tree = new();
    }

    /// <summary>
    /// Loads the metadata for the part of the same name from the backup with the given ID into the LastKnownState tree.
    /// </summary>
    private void Load(string id)
    {
        if (!File.Exists($"{Server.Config.Backup.Directory}{id}/{PartName}/Metadata.txt"))
            return;

        Stack<BackupTree> stack = new();
        stack.Push(LastKnownState);
        using StreamReader reader = new($"{Server.Config.Backup.Directory}{id}/{PartName}/Metadata.txt");

        int read;
        while (true)
        {
            //key
            StringBuilder keyBuilder = new();
            read = reader.Read();
            while (read != -1 && (char)read != ',')
            {
                keyBuilder.Append((char)read);
                read = reader.Read();
            }
            if (read == -1)
                return;
            string key = keyBuilder.ToString().FromBase64();

            //value
            var top = stack.Peek();
            read = reader.Read();
            switch (read)
            {
                case -1:
                    return;
                case '(':
                    //directory (not null)
                    if ((!top.Directories.TryGetValue(key, out var childDir)) || childDir == null)
                    {
                        childDir = new();
                        top.Directories[key] = childDir;
                    }
                    stack.Push(childDir);
                    continue;
                case '#':
                    //directory (null)
                    top.Directories.Remove(key);
                    read = reader.Read();
                    switch (read)
                    {
                        case -1:
                            return;
                        case ')':
                            stack.Pop();
                            continue;
                        default:
                            continue;
                    }
                case '-':
                    //file (null)
                    top.Files.Remove(key);
                    read = reader.Read();
                    switch (read)
                    {
                        case -1:
                            return;
                        case ')':
                            stack.Pop();
                            continue;
                        default:
                            continue;
                    }
            }
            //file (not null)
            StringBuilder valueBuilder = new();
            while (read != -1 && !";)".Contains((char)read))
            {
                valueBuilder.Append((char)read);
                read = reader.Read();
            }
            top.Files[key] = valueBuilder.ToString();
            if ((char)read == ')')
                stack.Pop();
        }
    }

    /// <summary>
    /// Writes the encoded tree of changes / states to a metadata file to finish this part of the backup.
    /// </summary>
    public void Finish()
    {
        File.WriteAllText($"{Server.Config.Backup.Directory}{BackupId}/{PartName}/Metadata.txt", Tree.Encode());
    }
}