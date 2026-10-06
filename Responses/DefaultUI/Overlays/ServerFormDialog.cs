using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI server form dialog.
/// </summary>
public class ServerFormDialog : Dialog, IActionHaver
{
    public ActionHandlerAsync Action { get; set; }
    
    public ServerFormDialog(string id, IconAndText heading, bool isOpen, IEnumerable<AbstractElement> items, ActionHandlerAsync action) : base(id, heading, isOpen, items)
    {
        Action = action;
        FixedAttributes.Add(("class", "wf-server-form"));
        FixedAttributes.Add(("method", "post"));
        FixedAttributes.Add(("enctype", "multipart/form-data"));
        FixedAttributes.Add(("action", "#"));
    }
    
    public ServerFormDialog(string id, IconAndText heading, bool isOpen, IEnumerable<AbstractElement> items, ActionHandler action)
        : this(id, heading, isOpen, items, action.ToAsync()) { }

    public override string RenderedTag
        => "form";
}