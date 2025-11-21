namespace uwap.WebFramework.Database;

public delegate void SetFileDelegate(string targetFilePath);

/// <summary>
/// A database file action to set the content of the file with the given file ID using the given writer function.
/// </summary>
public class SetFileAction(string fileId, SetFileDelegate writer) : IFileAction
{
    private readonly string FileId = fileId;
    
    private readonly SetFileDelegate Writer = writer;
    
    public void Prepare(AbstractTableValue value)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var filePath = entry.GetFilePath(FileId);
        var trashFilePath = entry.GetTrashFilePath(FileId);
        var bufferFilePath = entry.GetBufferFilePath(FileId);
        
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
        Writer(bufferFilePath);
    }
    
    public void Commit(AbstractTableValue value, long timestamp)
    {
        var entry = value.ContainingEntry ?? throw new Exception("Containing entry was not set.");
        var filePath = entry.GetFilePath(FileId);
        var trashFilePath = entry.GetTrashFilePath(FileId);
        var bufferFilePath = entry.GetBufferFilePath(FileId);
        
        // trash
        if (File.Exists(trashFilePath))
            File.Delete(trashFilePath);
        
        // buffer
        if (File.Exists(bufferFilePath))
            File.Move(bufferFilePath, filePath);
        
        // metadata
        value.Files[FileId] = new(timestamp, new FileInfo(filePath).Length);
    }
}