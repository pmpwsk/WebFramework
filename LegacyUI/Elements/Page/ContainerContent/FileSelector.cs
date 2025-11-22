namespace uwap.WebFramework.Elements;

/// <summary>
/// Button to select a file to upload for a container.
/// </summary>
public class FileSelector : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "input";

    //documentation inherited from IElement
    protected override string? ElementProperties => $"type=\"file\"{(AllowMultiple ? " multiple" : "")}{(OnChange == null ? "" : $" onchange=\"{OnChange.HtmlValueSafe()}\"")}";

    /// <summary>
    /// Whether to allow the selection of multiple files.
    /// </summary>
    public bool AllowMultiple;

    /// <summary>
    /// JavaScript command that should be executed when the value changes.
    /// </summary>
    public string? OnChange = null;

    /// <summary>
    /// Creates a new file selector element with the given ID for a container.
    /// </summary>
    public FileSelector(string id, bool allowMultiple = false)
    {
        Id = id;
        AllowMultiple = allowMultiple;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return CodeWithoutExplicitCloser;
    }
}