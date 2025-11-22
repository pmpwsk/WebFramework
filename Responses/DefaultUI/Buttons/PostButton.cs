using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button with an integrated form.
/// </summary>
public class PostButton : AbstractButton
{
    private readonly RequiredWatchedAttribute ActionAttribute;
    
    private readonly RequiredWatchedContainer<SubmitButton> SubmitContainer;
    
    public PostButton(string text, string action)
    {
        ActionAttribute = new(this, "action", action);
        SubmitContainer = new(this, new(text));
    }
    
    /// <summary>
    /// The URL of the action.
    /// </summary>
    public string Action
    {
        get => ActionAttribute.Value;
        set => ActionAttribute.Value = value;
    }
    
    /// <summary>
    /// The actual submit button.
    /// </summary>
    public SubmitButton Submit
    {
        get => SubmitContainer.Element;
        set => SubmitContainer.Element = value;
    }
    
    /// <summary>
    /// The button's text.
    /// </summary>
    public string Text
    {
        get => Submit.Text;
        set => Submit.Text = value;
    }
    
    public override string RenderedTag
        => "form";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ..base.RenderedContainers,
            SubmitContainer
        ];

    public override IEnumerable<(string Name, string? Value)> FixedAttributes
        => [
            ..base.FixedAttributes,
            ("method", "post"),
            ("enctype", "multipart/form-data")
        ];

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            ActionAttribute
        ];
}