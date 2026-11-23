using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI multi-line text input that expands and shrinks as lines are added or removed.
/// </summary>
public class FlexibleTextArea : AbstractInputWithFocus
{
    private readonly RequiredWatchedAttribute NameAttribute;
    
    private readonly OptionalWatchedAttribute PlaceholderAttribute;
    
    private readonly OptionalWatchedAttribute AutofocusAttribute;
    
    private readonly OptionalWatchedAttribute IsEmptyAttribute;
    
    private readonly ListWatchedContainer<AbstractMarkdownPart> ValueContainer;
    
    /// <summary>
    /// The last known value.
    /// </summary>
    public string Value { get; private set; }
    
    public FlexibleTextArea(string name, string? placeholder, string? initialValue, bool autofocus = false)
    {
        NameAttribute = new(this, "name", name);
        PlaceholderAttribute = new(this, "data-wf-placeholder", placeholder);
        AutofocusAttribute = new(this, "autofocus", autofocus ? "" : null);
        Value = initialValue?.Trim() ?? "";
        IsEmptyAttribute = new(this, "data-wf-is-empty", Value.Length == 0 ? "" : null);
        ValueContainer = new(this, Value.Length == 0 ? [] : LineBreak.Convert([Value]));
        FixedAttributes.Add(("class", "wf-textbox"));
        FixedAttributes.Add(("role", "textbox"));
        FixedAttributes.Add(("contenteditable", ""));
    }

    public override string RenderedTag
        => "span";
    
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

    public override bool Autofocus
    {
        get => AutofocusAttribute.Value == "";
        set => AutofocusAttribute.Value = value ? "" : null;
    }

    public override void SetValueFromForm(string input)
    {
        Value = input.Trim();
        ValueContainer.ReplaceAllWithoutNotifying(Value.Length == 0 ? [] : LineBreak.Convert([Value]));
        IsEmptyAttribute.SetValueWithoutNotifying(Value.Length == 0 ? "" : null);
    }
    
    /// <summary>
    /// Requests that the browser sets the current value to the specified value.
    /// </summary>
    public Task SetValueForClientAsync(string value)
    {
        Value = value.Trim();
        ValueContainer.ReplaceAllWithoutNotifying(Value.Length == 0 ? [] : LineBreak.Convert([Value]));
        IsEmptyAttribute.SetValueWithoutNotifying(Value.Length == 0 ? "" : null);
        return ChangeWatcher?.SetValueAsync(this, Value) ?? Task.CompletedTask;
    }
    
    public override void Focus()
        => ChangeWatcher?.FocusElement(this);
}