using System.Collections.ObjectModel;
using System.Text;

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
        foreach (string id in ids)
            Load(id);
    }

    private static void LoadRecursive(StreamReader reader, BackupTree tree, string id)
    {
        int read;

        while (true)
        {
            //key
            StringBuilder keyBuilder = new();
            read = reader.Read();
            if ((char)read == ')')
                return;
            while (read != -1 && (char)read != ',')
            {
                keyBuilder.Append((char)read);
                read = reader.Read();
            }
            if (read == -1)
                return;
            string key = keyBuilder.ToString();

            //value
            read = reader.Read();
            switch ((char)read)
            {
                case '(': //directory (not null)
                    if ((!tree.Directories.TryGetValue(key, out var subTree)) || subTree == null)
                    {
                        subTree = new();
                        tree.Directories[key] = subTree;
                    }
                    LoadRecursive(reader, subTree, id);
                    read = reader.Read();
                    break;
                case '#': //directory (null)
                    tree.Directories.Remove(key);
                    read = reader.Read();
                    break;
                case '-': //file (null)
                    tree.Files.Remove(key);
                    read = reader.Read();
                    break;
                default: //file (not null)
                    while (read != -1 && !";)".Contains((char)read))
                        read = reader.Read();
                    tree.Files[key] = id;
                    break;
            }
            switch (read)
            {
                case -1:
                case ')':
                    return;
                    //the only possible alternative is a ; but nothing needs to be done in that case
            }
        }
    }

    /// <summary>
    /// Loads the metadata for the part of the same name from the backup with the given ID into the LastKnownState tree.
    /// </summary>
    private void Load(string id)
    {
        if (!File.Exists($"{Server.Config.Backup.Directory}{id}/{PartName}/Metadata.txt"))
            return;

        using StreamReader reader = new($"{Server.Config.Backup.Directory}{id}/{PartName}/Metadata.txt");

        LoadRecursive(reader, OriginIdTree, id);
    }

    /// <summary>
    /// Restores the loaded backup part.
    /// </summary>
    public void Restore()
        => Restore("", "", OriginIdTree);

    private bool Restore(string originDirRel, string targetDir, BackupTree tree)
    {
        bool directoryCreated = false;

        //directories
        foreach (var d in tree.Directories)
        {
            if (d.Value == null)
                continue;
            if (Restore($"{originDirRel}{d.Key}/", $"{targetDir}{d.Key.FromBase64()}/", d.Value))
                directoryCreated = true;
        }

        //files
        foreach (var f in tree.Files)
        {
            if (!directoryCreated)
            {
                Directory.CreateDirectory(targetDir);
                directoryCreated = true;
            }
            File.Copy($"{Server.Config.Backup.Directory}{f.Value}/{PartName}/{originDirRel}{f.Key}", $"{targetDir}{f.Key.FromBase64()}");
        }

        return directoryCreated;
    }
}