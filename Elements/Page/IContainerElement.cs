namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class for container elements.
/// </summary>
public abstract class IContainerElement : IPageElement
{
    //documentation inherited from IElement
    protected override string ElementType => "div";

    //documentation inherited from IElement
    protected override string? ElementProperties => null;

    /// <summary>
    /// The container's title or null to disable.
    /// </summary>
    public string? Title;

    /// <summary>
    /// The list of contents.<br/>
    /// Default: empty list
    /// </summary>
    public List<IContent> Contents = [];

    /// <summary>
    /// The list of buttons.<br/>
    /// Default: empty list
    /// </summary>
    public List<IButton> Buttons = [];
    
    /// <summary>
    /// Sets the list of buttons to a list with only the given button in it.
    /// </summary>
    public IButton Button
    {
        set => Buttons = [value];
    }
}