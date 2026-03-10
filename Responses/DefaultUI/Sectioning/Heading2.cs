namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The second-largest default UI heading.
/// </summary>
public class Heading2(IconAndText content) : AbstractHeading(content)
{
    public override string RenderedTag
        => "h2";
}