using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI form subsection that executes a server action instead of posting to a URL.
/// </summary>
public class ServerForm(string? heading, ActionHandler action, IEnumerable<AbstractElement>? content = null) : AbstractSubsection(heading, content)
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public ActionHandler Action = action;
    
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