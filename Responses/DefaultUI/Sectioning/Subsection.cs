using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI subsection.
/// </summary>
public class Subsection(string? heading, IEnumerable<AbstractElement>? content = null) : AbstractSubsection(heading, content)
{
    public override string RenderedTag
        => "div";
}