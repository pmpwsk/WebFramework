namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class for elements for Page.
/// </summary>
public abstract class IPageElement : IElement
{
    //documentation inherited from IElement
    protected override string? ElementClass => "elem";

    /// <summary>
    /// Enumerates the lines of the page element.<br/>
    /// In implementations, this should happen without first saving the code as a list of strings or anything similar. It should be generated and sent out immediately.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<string> Export();
}