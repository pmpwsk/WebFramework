using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI form subsection that executes a server action instead of posting to a URL.
/// </summary>
public class ServerForm : AbstractSubsection
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public Func<IResponse> Action = () => StatusResponse.Success;
    
    public ServerForm(string? heading, IEnumerable<AbstractElement>? content = null) : base(heading, content)
    {
    }
    
    public override string RenderedTag
        => "form";

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-server-form"),
            ("method", "post"),
            ("enctype", "multipart/form-data"),
            ("action", "#")
        ];
}