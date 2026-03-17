using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A simple flex column.
/// </summary>
public class Column : OptionalIdElement
{
    public readonly ListWatchedContainer<AbstractElement> Content;
    
    public Column(IEnumerable<AbstractElement>? content = null)
    {
        Content = new(this, content ?? []);
        FixedAttributes.Add(("class", "wf-col"));
    }
        
    public override string RenderedTag
        => "div";
}