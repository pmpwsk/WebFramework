namespace uwap.Database;

/// <summary>
/// A database file action to change the file ID of a file.
/// </summary>
public class MoveFileAction(string oldFileId, string newFileId) : IFileAction
{
    private readonly string OldFileId = oldFileId;
    private readonly string NewFileId = newFileId;
    
    public void Prepare(AbstractTableValue value)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var oldFilePath = entry.GetFilePath(OldFileId);
        var filePath = entry.GetFilePath(NewFileId);
        var trashFilePath = entry.GetTrashFilePath(NewFileId);
        var bufferFilePath = entry.GetBufferFilePath(NewFileId);
        
        Directory.CreateDirectory(entry.FileBasePath);
        
        // trash
        Directory.CreateDirectory(entry.TrashFileBasePath);
        if (File.Exists(trashFilePath))
            File.Delete(trashFilePath);
        if (File.Exists(filePath))
            File.Move(filePath, trashFilePath);
        
        // buffer
        Directory.CreateDirectory(entry.BufferFileBasePath);
        if (File.Exists(bufferFilePath))
            File.Delete(bufferFilePath);
        
        File.Move(oldFilePath, bufferFilePath);
    }
    
    public void Commit(AbstractTableValue value, long timestamp)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var filePath = entry.GetFilePath(NewFileId);
        var trashFilePath = entry.GetTrashFilePath(NewFileId);
        var bufferFilePath = entry.GetBufferFilePath(NewFileId);
        
        // trash
        if (File.Exists(trashFilePath))
            File.Delete(trashFilePath);
        
        // buffer
        if (File.Exists(bufferFilePath))
            File.Move(bufferFilePath, filePath);
        
        // metadata
        value.Files[NewFileId] = new(timestamp, new FileInfo(filePath).Length);
        value.Files.Remove(OldFileId);
    }
}