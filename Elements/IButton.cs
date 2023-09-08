namespace uwap.WebFramework.Elements;

public abstract class IButton : IElement
{
    protected override string ElementType => "a";
    protected override string? ElementClass => null;

    public string? Text = null;

    public string Export()
        => Opener + (Text??"Button") + Closer;
}