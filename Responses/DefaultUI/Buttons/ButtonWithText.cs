using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI button with a text.
/// </summary>
public abstract class ButtonWithText : AbstractButton
{
    /// <summary>
    /// The object describing the button's text.
    /// </summary>
    private readonly RequiredWatchedContainer<MarkdownText> ContentContainer;
    
    protected ButtonWithText(string text)
    {
        ContentContainer = new(this, new(text));
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public string Text
    {
        get => ContentContainer.Element.Text;
        set => ContentContainer.Element = new(value);
    }

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ContentContainer
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("class", "wf-button")
        ];
}