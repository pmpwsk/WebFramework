using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI form subsection that executes a server action instead of posting to a URL.
/// </summary>
public class ServerForm : AbstractSubsection, IActionHaver
{
    public ActionHandler Action { get; set; }

    public ServerForm(string? heading, ActionHandler action, IEnumerable<AbstractElement>? content = null) : base(heading, content)
    {
        Action = action;
        FixedAttributes.Add(("class", "wf-server-form"));
        FixedAttributes.Add(("method", "post"));
        FixedAttributes.Add(("enctype", "multipart/form-data"));
        FixedAttributes.Add(("action", "#"));
    }

    public override string RenderedTag
        => "form";
}