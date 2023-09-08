namespace uwap.WebFramework.Elements;

public abstract class IPageElement : IElement
{
    protected override string? ElementClass => "elem";
    public abstract ICollection<string> Export();
}