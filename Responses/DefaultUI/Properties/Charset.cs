using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A character set metadata tag for UTF-8.
/// </summary>
public class Charset : WatchedElement
{
    public override string RenderedTag
        => "meta";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("charset", "utf-8")
        ];
}