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
    /// Whether the current backup is a fresh backup.
    /// </summary>
    public readonly bool BackupFresh;

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

        BackupFresh = backupBasedOnIds.Count == 0;

        LastKnownState = new();
        foreach (string backupBasedOnId in backupBasedOnIds)
            Load(backupBasedOnId);
        
        Tree = new();
    }

    private static void LoadRecursive(StreamReader reader, BackupTree tree)
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
                    LoadRecursive(reader, subTree);
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
                    StringBuilder valueBuilder = new();
                    while (read != -1 && !";)".Contains((char)read))
                    {
                        valueBuilder.Append((char)read);
                        read = reader.Read();
                    }
                    tree.Files[key] = valueBuilder.ToString();
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

        LoadRecursive(reader, LastKnownState);
    }

    /// <summary>
    /// Backs up the file at the given path if it has been modified since the last backup.
    /// </summary>
    public void BackupFile(string path)
    {
        var componentsB64 = path.Split('/', '\\').Select(x => x.ToBase64()).ToArray();

        //known timestamp
        string? knownTimestamp;
        var tree = LastKnownState;
        foreach (var componentB64 in componentsB64.SkipLast(1))
            if (tree.Directories.TryGetValue(componentB64, out var d) && d != null)
                tree = d;
            else
            {
                knownTimestamp = null;
                goto knownTimestampDone;
            }
        if (tree.Files.TryGetValue(componentsB64.Last(), out var t))
            knownTimestamp = t;
        else knownTimestamp = null;
    knownTimestampDone:

        //current timestamp, stop if up to date already
        string currentTimestamp = File.GetLastWriteTimeUtc(path).Ticks.ToString();
        if (knownTimestamp == currentTimestamp)
            return;

        //add timestamp to the tree
        tree = Tree;
        foreach (var componentB64 in componentsB64.SkipLast(1))
        {
            if ((!tree.Directories.TryGetValue(componentB64, out var d)) || d == null)
            {
                d = new();
                tree.Directories[componentB64] = d;
            }
            tree = d;
        }
        tree.Files[componentsB64.Last()] = currentTimestamp;

        //create directory if it doesn't exist yet
        Directory.CreateDirectory($"{Server.Config.Backup.Directory}{BackupId}/{PartName}/{string.Join('/', componentsB64.SkipLast(1))}");

        //copy file
        File.Copy(path, $"{Server.Config.Backup.Directory}{BackupId}/{PartName}/{string.Join('/', componentsB64)}");
    }

    /// <summary>
    /// Backs up the entire directory at the given path if it has been modified since the last backup.
    /// </summary>
    public void BackupDirectory(string path)
    {
        var componentsB64 = path.Split('/', '\\').Select(x => x.ToBase64()).ToArray();

        //known tree
        BackupTree? knownTree = LastKnownState;
        foreach (var componentB64 in componentsB64)
            if (knownTree.Directories.TryGetValue(componentB64, out var d) && d != null)
                knownTree = d;
            else
            {
                knownTree = null;
                goto knownTreeDone;
            }
        knownTreeDone:

        //current tree
        BackupTree? lastExistingTree = null;
        BackupTree? firstMissingTree = null;
        string? missingTreeB64 = null;
        BackupTree currentTree = Tree;
        foreach (var componentB64 in componentsB64)
            if (lastExistingTree == null)
            {
                if (currentTree.Directories.TryGetValue(componentB64, out var d) && d != null)
                    currentTree = d;
                else
                {
                    lastExistingTree = currentTree;
                    firstMissingTree = new();
                    missingTreeB64 = componentB64;
                    currentTree = firstMissingTree;
                }
            }
            else
            {
                BackupTree d = new();
                currentTree.Directories[componentB64] = d;
                currentTree = d;
            }

        //run the backup
        BackupDirectory(new DirectoryInfo(path), knownTree, currentTree, $"{Server.Config.Backup.Directory}{BackupId}/{PartName}/{string.Join('/', componentsB64)}");

        //add tree if necessary
        if (lastExistingTree != null && missingTreeB64 != null && firstMissingTree != null && (BackupFresh || currentTree.Files.Count != 0 || currentTree.Directories.Count != 0))
            lastExistingTree.Directories[missingTreeB64] = firstMissingTree;
    }

    private bool BackupDirectory(DirectoryInfo dir, BackupTree? knownTree, BackupTree currentTree, string backupToDir)
    {
        bool directoryCreated = false;

        //directories
        foreach (var d in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            string b64 = d.Name.ToBase64();

            //known tree
            if (knownTree == null || !knownTree.Directories.TryGetValue(b64, out BackupTree? k))
                k = null;

            //current tree
            bool potential;
            if (currentTree.Directories.TryGetValue(b64, out BackupTree? c) && c != null)
                potential = false;
            else
            {
                potential = true;
                c = new();
            }

            //run the backup
            if (BackupDirectory(d, k, c, $"{backupToDir}/{b64}"))
                directoryCreated = true;

            //add tree if necessary
            if (potential && (BackupFresh || c.Files.Count != 0 || c.Directories.Count != 0))
                currentTree.Directories[b64] = c;
        }

        //files
        foreach (var f in dir.GetFiles("*", SearchOption.TopDirectoryOnly))
        {
            string b64 = f.Name.ToBase64();

            //known timestamp
            if (knownTree == null || !knownTree.Files.TryGetValue(b64, out string? knownTimestamp))
                knownTimestamp = null;

            //current timestamp, skip if it's up to date already
            string currentTimestamp = f.LastWriteTimeUtc.Ticks.ToString();
            if (knownTimestamp == currentTimestamp)
                continue;

            //add timestamp to the tree
            currentTree.Files[b64] = currentTimestamp;

            //create directory if it doesn't exist yet
            if (!directoryCreated)
            {
                Directory.CreateDirectory(backupToDir);
                directoryCreated = true;
            }

            //copy file
            f.CopyTo($"{backupToDir}/{b64}");
        }

        return directoryCreated;
    }

    /// <summary>
    /// Finds deleted files/directories and writes the encoded tree of changes/states to a metadata file to finish this part of the backup.
    /// </summary>
    public void Finish()
    {
        //add missing files from last known to tree as deleted
        FindDeleted(LastKnownState, Tree, "");

        //write metadata
        File.WriteAllText($"{Server.Config.Backup.Directory}{BackupId}/{PartName}/Metadata.txt", Tree.Encode());
    }

    private static void FindDeleted(BackupTree knownTree, BackupTree currentTree, string path)
    {
        //directories
        foreach (var dKV in knownTree.Directories)
        {
            if (dKV.Value == null)
                continue;

            string name = dKV.Key.FromBase64();
            if (Directory.Exists($"{path}{name}"))
            {
                //current
                bool potential;
                if (currentTree.Directories.TryGetValue(dKV.Key, out BackupTree? c) && c != null)
                    potential = false;
                else
                {
                    potential = true;
                    c = new();
                }

                //find more
                FindDeleted(dKV.Value, c, $"{path}{name}/");

                //add tree if necessary
                if (potential && (c.Files.Count != 0 || c.Directories.Count != 0))
                    currentTree.Directories[dKV.Key] = c;
            }
            else currentTree.Directories[dKV.Key] = null;
        }

        //files
        foreach (var fKV in knownTree.Files)
        {
            string name = fKV.Key.FromBase64();
            if (!File.Exists($"{path}{name}"))
                currentTree.Files[fKV.Key] = null;
        }
    }
}