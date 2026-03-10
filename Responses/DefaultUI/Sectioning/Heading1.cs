namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The largest default UI heading.
/// </summary>
public class Heading1(IconAndText content) : AbstractHeading(content)
{
    public override string RenderedTag
        => "h1";
}