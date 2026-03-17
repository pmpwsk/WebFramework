using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A simple flex row.
/// </summary>
public class Row : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractElement> Content;
    
    public Row(IEnumerable<AbstractElement>? content = null)
    {
        Content = new(this, content ?? []);
        FixedAttributes.Add(("class", "wf-row"));
    }
        
    public override string RenderedTag
        => "div";
}