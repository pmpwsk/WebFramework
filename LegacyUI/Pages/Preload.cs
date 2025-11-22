namespace uwap.WebFramework.Elements;

/// <summary>
/// Contains information about a resource a page should reload.
/// </summary>
public class Preload
{
    /// <summary>
    /// The URL of the resource to preload.
    /// </summary>
    public string Url;

    /// <summary>
    /// The intended role of the resource according to the specification.
    /// </summary>
    public string As;

    /// <summary>
    /// The MIME type of the resource or null to omit the type.
    /// </summary>
    public string? Type;

    /// <summary>
    /// Whether the resource should be allowed to be used cross-origin.
    /// </summary>
    public bool Crossorigin;

    /// <summary>
    /// Creates a new preload object with the given information.
    /// </summary>
    /// <param name="url">The URL of the resource to preload.</param>
    /// <param name="loadAs">The intended role of the resource according to the specification.</param>
    /// <param name="type">The MIME type of the resource or null to omit the type.</param>
    /// <param name="crossorigin">Whether the resource should be allowed to be used cross-origin.</param>
    public Preload(string url, string loadAs, string? type = null, bool crossorigin = true)
    {
        Url = url;
        As = loadAs;
        Type = type;
        Crossorigin = crossorigin;

        if (Type == null)
        {
            string extension = Url;
            if (extension.Contains('/'))
                extension = extension.Remove(0, extension.LastIndexOf('/')+1);
            if (extension.Contains('.'))
                extension = extension.Remove(0, extension.LastIndexOf('.'));
            else extension = "";

            if (Server.Config.MimeTypes.TryGetValue(extension, out string? mimeType))
                Type = mimeType;
        }
    }

    /// <summary>
    /// Exports the preload as a single line.
    /// </summary>
    public string Export()
        => $"<link rel=\"preload\" href=\"{Url.HtmlValueSafe()}\"{(As==null?"":$" as=\"{As.HtmlValueSafe()}\"")}{(Type==null?"":$" type=\"{Type.HtmlValueSafe()}\"")}{(Crossorigin?" crossorigin":"")} />";
}