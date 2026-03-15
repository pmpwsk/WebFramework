namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI menu.
/// </summary>
public class Menu : AbstractOverlay
{
    public Menu(string id, IconAndText heading, IEnumerable<AbstractButton>? items = null) : base(id, heading, items)
    {
        FixedAttributes.Add(("class", "wf-menu"));
    }
}