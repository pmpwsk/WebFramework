namespace uwap.WebFramework.Elements;

public class CustomElement : IPageElement
{
    protected override string ElementType => "";
    protected override string? ElementClass => null;
    protected override string? ElementProperties => null;

    private List<string> HtmlCode;

    public CustomElement(string htmlCode)
        => HtmlCode = new List<string> { htmlCode };
    public CustomElement(List<string> htmlCode)
        => HtmlCode = htmlCode;

    public override ICollection<string> Export()
        => HtmlCode;
}