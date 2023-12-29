namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class for button elements, either for hyperlinks or JavaScript commands.
/// </summary>
public abstract class IButtonElement : IPageElement
{
    //documentation inherited from IElement
    protected override string ElementType => "a";

    /// <summary>
    /// The button's subtext or null to disable.
    /// </summary>
    public string? Text;

    /// <summary>
    /// The button's title or null to disable.
    /// </summary>
    public string? Title;

    //documentation inherited from IPageElement
    public override IEnumerable<string> Export()
    {
        yield return Opener;
        
        if (Title != null) yield return $"\t<h2>{Title.HtmlSafe()}</h2>";
        if (Text != null) yield return $"\t<p>{Text.HtmlSafe()}</p>";

        yield return Closer;
    }
}