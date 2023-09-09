namespace uwap.WebFramework.Elements;

/// <summary>
/// Button to select a file to upload for a container.
/// </summary>
public class FileSelector : IContent
{
    //not needed
    protected override string ElementType => "";
    
    /// <summary>
    /// Creates a new file selector element with the given ID for a container.
    /// </summary>
    public FileSelector(string id)
    {
        Id = id;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return $"<input type=\"file\" id=\"{Id}\">";
    }
}