namespace uwap.WebFramework.Elements;

public abstract class IContainerElement : IPageElement
{
    protected override string ElementType => "div";
    protected override string? ElementProperties => null;

    public string? Title;
    public List<IContent> Contents = new List<IContent>();
    public List<IButton> Buttons = new List<IButton>();
    
    public IButton Button
    {
        set
        {
            Buttons = new List<IButton> { value };
        }
    }
}