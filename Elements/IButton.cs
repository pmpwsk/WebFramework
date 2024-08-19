namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class for generic buttons, either for hyperlinks or JavaScript commands.
/// </summary>
public abstract class IButton : IElement
{
    //documentation inherited from IElement
    protected override string ElementType => "a";

    //documentation inherited from IElement
    protected override string? ElementClass => null;

    /// <summary>
    /// The text of the button.
    /// </summary>
    public string? Text = null;

    /// <summary>
    /// The button a line of HTML code.
    /// </summary>
    public string Export()
        => Opener + (Text??"Button").HtmlSafe(Unsafe) + Closer;
}