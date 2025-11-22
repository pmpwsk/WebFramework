namespace uwap.WebFramework.Elements;

/// <summary>
/// Custom page element with a list of code lines.
/// </summary>
public class CustomElement : IPageElement
{
    //not used
    protected override string ElementType => "";

    //not used
    protected override string? ElementClass => null;

    //not used
    protected override string? ElementProperties => null;

    /// <summary>
    /// The lines of HTML code of the element.
    /// </summary>
    public List<string> HtmlCode { get; set; }

    /// <summary>
    /// Creates a new custom page element with the given (first) line of code.
    /// </summary>
    public CustomElement(string htmlCode)
        => HtmlCode = [htmlCode];

    /// <summary>
    /// Creates a new custom page element with the given lines of code.
    /// </summary>
    public CustomElement(List<string> htmlCode)
        => HtmlCode = htmlCode;

    /// <summary>
    /// Creates a new custom page element with the given lines of code.
    /// </summary>
    public CustomElement(params string[] htmlCode)
        => HtmlCode = [.. htmlCode];

    //documentation inherited from IPageElement
    public override IEnumerable<string> Export()
    {
        foreach (string line in HtmlCode)
            yield return line;
    }
}