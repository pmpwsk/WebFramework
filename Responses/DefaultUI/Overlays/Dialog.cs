using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI dialog.
/// </summary>
public class Dialog : AbstractOverlay
{
    private readonly OptionalWatchedAttribute IsOpenAttribute;
    
    public Dialog(string id, IconAndText heading, bool isOpen, IEnumerable<AbstractElement>? items = null) : base(id, heading, items)
    {
        IsOpenAttribute = new(this, "class", isOpen ? "wf-is-open" : null);
        FixedAttributes.Add(("class", "wf-dialog"));
    }
    
    /// <summary>
    /// Whether the dialog is currently open.
    /// </summary>
    public bool IsOpen
    {
        get => IsOpenAttribute.Value == "wf-is-open";
        set => IsOpenAttribute.Value = value ? "wf-is-open" : null;
    }
}