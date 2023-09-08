namespace uwap.WebFramework.Elements;

public class LargeContainerElement : ContainerElement
{
    protected override bool Large => true;

    public LargeContainerElement(string? title, string text, string? classes = null, string? styles = null, string? id = null)
        : base(title, text, classes, styles, id) { }

    public LargeContainerElement(string? title, IContent? content, string? classes = null, string? styles = null, string? id = null)
        : base(title, content, classes, styles, id) { }

    public LargeContainerElement(string? title, List<IContent>? contents, string? classes = null, string? styles = null, string? id = null)
        : base(title, contents, classes, styles, id) { }
}