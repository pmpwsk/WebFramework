using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A line break.
/// </summary>
public class LineBreak : AbstractElement
{
    public override string RenderedTag
        => "br";
}