using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An inline text part that represents an icon with a CSS class.
/// </summary>
public class Icon : OptionalIdElement
{
    public Icon(string className)
    {
        Class = className;
        FixedAttributes.Add(("class", "wf-icon"));
    }
    
    public override string RenderedTag
        => "i";
}