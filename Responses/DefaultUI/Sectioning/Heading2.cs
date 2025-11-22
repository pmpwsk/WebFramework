namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The second-largest default UI heading.
/// </summary>
public class Heading2(string text) : AbstractHeading(text)
{
    public override string RenderedTag
        => "h2";
}