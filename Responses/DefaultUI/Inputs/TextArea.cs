using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI multi-line text input.
/// </summary>
public class TextArea : AbstractInputWithFocus
{
    private readonly RequiredWatchedAttribute NameAttribute;
    
    private readonly OptionalWatchedAttribute PlaceholderAttribute;
    
    private readonly OptionalWatchedAttribute AutofocusAttribute;
    
    private readonly OptionalWatchedAttribute RowsAttribute;
    
    private readonly OptionalWatchedContainer<MarkdownText> InitialValueContainer;
    
    /// <summary>
    /// The last known value.
    /// </summary>
    public string Value { get; private set; }
    
    public TextArea(string name, string? placeholder, string? initialValue, int? rows, bool autofocus = false)
    {
        NameAttribute = new(this, "name", name);
        PlaceholderAttribute = new(this, "placeholder", placeholder);
        AutofocusAttribute = new(this, "autofocus", autofocus ? "" : null);
        Value = initialValue ?? "";
        RowsAttribute = new(this, "rows", rows == null ? null : rows.ToString());
        InitialValueContainer = new(this, initialValue == null ? null : new(initialValue));
        FixedAttributes.Add(("class", "wf-textbox"));
    }

    public override string RenderedTag
        => "textarea";
    
    public override bool IsEmpty
        => string.IsNullOrWhiteSpace(Value);
    
    /// <summary>
    /// Returns null if the current value is unknown or empty, otherwise the value itself.
    /// </summary>
    public string? ValueNullable
        => string.IsNullOrWhiteSpace(Value) ? null : Value;

    /// <summary>
    /// The input's name, used for browser suggestions.
    /// </summary>
    public string Name
    {
        get => NameAttribute.Value;
        set => NameAttribute.Value = value;
    }

    public string? Placeholder
    {
        get => PlaceholderAttribute.Value;
        set => PlaceholderAttribute.Value = value;
    }
    
    /// <summary>
    /// The initial value of the text box.
    /// </summary>
    public string? InitialValue
    {
        get => InitialValueContainer.Element?.Text;
        set => InitialValueContainer.Element = value == null ? null : new(value);
    }

    public override bool Autofocus
    {
        get => AutofocusAttribute.Value == "";
        set => AutofocusAttribute.Value = value ? "" : null;
    }

    public override void SetValueFromForm(string input)
    {
        Value = input;
        InitialValueContainer.SetElementWithoutNotifying(Value == "" ? null : new(Value));
    }
    
    /// <summary>
    /// Requests that the browser sets the current value to the specified value.
    /// </summary>
    public Task SetValueForClientAsync(string value)
    {
        Value = value;
        InitialValueContainer.SetElementWithoutNotifying(Value == "" ? null : new(Value));
        return ChangeWatcher?.SetValueAsync(this, value) ?? Task.CompletedTask;
    }
    
    public override void Focus()
        => ChangeWatcher?.FocusElement(this);
}