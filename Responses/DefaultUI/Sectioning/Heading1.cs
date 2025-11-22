namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The largest default UI heading.
/// </summary>
public class Heading1(string text) : AbstractHeading(text)
{
    public override string RenderedTag
        => "h1";
}