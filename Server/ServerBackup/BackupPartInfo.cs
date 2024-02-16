﻿using System.Collections.ObjectModel;
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
    public void BackupDirectory(string path)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes the encoded tree of changes / states to a metadata file to finish this part of the backup.
    /// </summary>
    public void Finish()
    {
        File.WriteAllText($"{Server.Config.Backup.Directory}{BackupId}/{PartName}/Metadata.txt", Tree.Encode());
    }
}