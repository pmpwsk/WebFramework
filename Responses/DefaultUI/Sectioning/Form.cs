using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI form subsection.
/// </summary>
public class Form : AbstractSubsection
{
    private readonly RequiredWatchedAttribute ActionAttribute;
    
    public Form(string? heading, string action, IEnumerable<AbstractElement>? content = null) : base(heading, content)
    {
        ActionAttribute = new(this, "action", action);
        FixedAttributes.Add(("method", "post"));
        FixedAttributes.Add(("enctype", "multipart/form-data"));
    }
    
    public override string RenderedTag
        => "form";
    
    /// <summary>
    /// The URL of the action.
    /// </summary>
    public string Action
    {
        get => ActionAttribute.Value;
        set => ActionAttribute.Value = value;
    }
}