using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A character set metadata tag for UTF-8.
/// </summary>
public class Charset : WatchedElement
{
    public Charset()
    {
        FixedAttributes.Add(("charset", "utf-8"));
    }
    
    public override string RenderedTag
        => "meta";
}