namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The third-largest default UI heading.
/// </summary>
public class Heading3(string text) : AbstractHeading(text)
{
    public override string RenderedTag
        => "h3";
}