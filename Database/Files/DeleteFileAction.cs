namespace uwap.WebFramework.Database;

/// <summary>
/// A database file action to delete the file with the given file ID.
/// </summary>
public class DeleteFileAction(string fileId) : IFileAction
{
    private readonly string FileId = fileId;
    
    public void Prepare(AbstractTableValue value)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var filePath = entry.GetFilePath(FileId);
        var trashFilePath = entry.GetTrashFilePath(FileId);
        
        if (File.Exists(trashFilePath))
            File.Delete(trashFilePath);
        Directory.CreateDirectory(entry.TrashFileBasePath);
        if (File.Exists(filePath))
            File.Move(filePath, trashFilePath);
    }
    
    public void Commit(AbstractTableValue value, long timestamp)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var trashFilePath = entry.GetTrashFilePath(FileId);
        
        if (File.Exists(trashFilePath))
            File.Delete(trashFilePath);
        
        value.Files.Remove(FileId);
    }
}