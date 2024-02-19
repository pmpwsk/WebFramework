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

    /// <summary>
    /// Loads the metadata for the part of the same name from the backup with the given ID into the LastKnownState tree.
    /// </summary>
    private void Load(string id)
    {
        if (!File.Exists($"{Server.Config.Backup.Directory}{id}/{PartName}/Metadata.txt"))
            return;

        Stack<BackupTree> stack = new();
        stack.Push(OriginIdTree);
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
            string key = keyBuilder.ToString();

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
            while (read != -1 && !";)".Contains((char)read))
                read = reader.Read();
            top.Files[key] = id;
            while ((char)read == ')')
            {
                stack.Pop();
                read = reader.Read();
            }
        }
    }

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