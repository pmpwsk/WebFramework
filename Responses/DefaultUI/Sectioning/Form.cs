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

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ActionAttribute
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("method", "post"),
            ("enctype", "multipart/form-data")
        ];
}