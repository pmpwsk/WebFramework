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

    /// <summary>
    /// Encodes the tree for a metadata file.
    /// </summary>
    public string Encode()
        => string.Join(';',
            [
                .. Files.Select(x => $"{x.Key.ToBase64()},{x.Value??"-"}"),
                .. Directories.Select(x => $"{x.Key.ToBase64()},{x.Value?.Encode()??"#"}")
            ]);
}