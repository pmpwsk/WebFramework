namespace uwap.WebFramework.Elements;

/// <summary>
/// A type of container content that's a span pretending to be a textarea, so that it's a textarea growing with the text within it.<br/>
/// Note that there are some limitations of this approach, like the inability to have a placeholder without additional JavaScript code.
/// </summary>
public class FlexibleTextArea : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "span";

    //documentation inherited from IElement
    protected override string? ElementClass => "textarea";

    //documentation inherited from IElement
    protected override string? ElementProperties => $"role=\"textbox\" contenteditable{(Autofocus?" autofocus":"")}{(OnInput!=null?$" oninput=\"{OnInput.HtmlValueSafe()}\"":"")}";

    /// <summary>
    /// The text that is in the text area by default or null to disable.
    /// </summary>
    public string? Text = null;

    /// <summary>
    /// Whether to automatically focus on this text area when the page loads.
    /// </summary>
    public bool Autofocus;

    /// <summary>
    /// JavaScript command that should be executed when the text changes.
    /// </summary>
    public string? OnInput = null;

    /// <summary>
    /// Creates a new flexible text area for a container.
    /// </summary>
    public FlexibleTextArea(string? text, string id, string? classes = null, string? styles = null, bool autofocus = false, string? onInput = null)
    {
        Text = text;
        Id = id;
        Class = classes;
        Style = styles;
        Autofocus = autofocus;
        OnInput = onInput;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener + (Text == null ? "" : Text.HtmlSafe()) + Closer;
    }
}