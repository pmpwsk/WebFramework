namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The third-largest default UI heading.
/// </summary>
public class Heading3(IconAndText content) : AbstractHeading(content)
{
    public override string RenderedTag
        => "h3";
}