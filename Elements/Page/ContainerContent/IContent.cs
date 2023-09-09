namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class to summarize elements that can be used as the content of a container element.
/// </summary>
public abstract class IContent : IElement
{
    /// <summary>
    /// The class of the content element.
    /// </summary>
    protected override string? ElementClass => null;

    /// <summary>
    /// The additional properties of the content element.
    /// </summary>
    protected override string? ElementProperties => null;

    /// <summary>
    /// Enumerates the lines of generated code for the content element.<br/>
    /// In implementations, this should happen without first saving the code as a list of strings or anything similar. It should be generated and sent out immediately.
    /// </summary>
    public abstract IEnumerable<string> Export();
}