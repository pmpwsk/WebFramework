namespace uwap.WebFramework.Elements;

public abstract class IContent : IElement
{
    protected override string? ElementClass => null;
    protected override string? ElementProperties => null;
    
    public abstract ICollection<string> Export();
}