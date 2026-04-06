using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI server form dialog.
/// </summary>
public class ServerFormDialog : Dialog, IActionHaver
{
    public ActionHandler Action { get; set; }
    
    public ServerFormDialog(string id, IconAndText heading, bool isOpen, IEnumerable<AbstractElement> items, ActionHandler action) : base(id, heading, isOpen, items)
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