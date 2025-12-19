using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI loading screen.
/// </summary>
public class LoadingScreen : WatchedElement
{
    private readonly OptionalWatchedAttribute IsOpenAttribute;
    
    /// <summary>
    /// The loading animation element.
    /// </summary>
    private readonly RequiredWatchedContainer<CustomElement> Content;
    
    public LoadingScreen(bool isOpen)
    {
        IsOpenAttribute = new(this, "class", isOpen ? "wf-is-open" : null);
        Content = new(this, new("div", [ new CustomElement("div") { Class = "wf-loading-content" } ]) { Class = "wf-loading-container" });
        FixedAttributes.Add(("class", "wf-loading-screen"));
    }
    
    internal override string? FixedSystemId
        => "loading";
    
    public override string RenderedTag
        => "div";
    
    /// <summary>
    /// Whether the loading screen is currently shown.
    /// </summary>
    public bool IsOpen
    {
        get => IsOpenAttribute.Value == "wf-is-open";
        set => IsOpenAttribute.Value = value ? "wf-is-open" : null;
    }
}