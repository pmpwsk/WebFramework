using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Possible roles for text input boxes.
/// </summary>
public enum TextBoxRole
{
    /// <summary>
    /// Input for normal text.
    /// </summary>
    Normal,

    /// <summary>
    /// Input for text without spell checking.
    /// </summary>
    NoSpellcheck,

    /// <summary>
    /// Input for usernames.
    /// </summary>
    Username,

    /// <summary>
    /// Input for existing passwords.
    /// </summary>
    CurrentPassword,

    /// <summary>
    /// Input for new passwords.
    /// </summary>
    NewPassword,

    /// <summary>
    /// Input for email addresses.
    /// </summary>
    Email,

    /// <summary>
    /// Input for phone numbers.
    /// </summary>
    Phone
}

/// <summary>
/// A default UI single-line text input.
/// </summary>
public class TextBox : AbstractInput
{
    private readonly RequiredWatchedAttribute NameAttribute;
    
    private readonly OptionalWatchedAttribute PlaceholderAttribute;
    
    private readonly OptionalWatchedAttribute InitialValueAttribute;
    
    private readonly OptionalWatchedAttribute AutofocusAttribute;
    
    private readonly RequiredWatchedAttribute TypeAttribute;
    
    private readonly OptionalWatchedAttribute SpellcheckAttribute;
    
    private readonly OptionalWatchedAttribute AutocompleteAttribute;
    
    private TextBoxRole _Role;
    
    /// <summary>
    /// The last known value.
    /// </summary>
    public string Value { get; private set; }
    
    public TextBox(string name, string? placeholder, string? initialValue, TextBoxRole role)
    {
        NameAttribute = new(this, "name", name);
        PlaceholderAttribute = new(this, "placeholder", placeholder);
        InitialValueAttribute = new(this, "value", initialValue);
        AutofocusAttribute = new(this, "autofocus", null);
        Value = initialValue ?? "";
        _Role = role;
        var config = ConfigurationFromRole(role);
        TypeAttribute = new(this, "type", config.Type);
        SpellcheckAttribute = new(this, "spellcheck", config.Spellcheck);
        AutocompleteAttribute = new(this, "autocomplete", config.Autocomplete);
        FixedAttributes.Add(("class", "wf-textbox"));
    }

    public override string RenderedTag
        => "input";

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
        get => InitialValueAttribute.Value;
        set
        {
            InitialValueAttribute.Value = value;
            Value = value ?? "";
        }
    }
    
    /// <summary>
    /// The input's configuration.
    /// </summary>
    public TextBoxRole Role
    {
        get => _Role;
        set
        {
            _Role = value;
            (TypeAttribute.Value, SpellcheckAttribute.Value, AutocompleteAttribute.Value) = ConfigurationFromRole(value);
        }
    }

    /// <summary>
    /// Whether the input's value should be censored like a password.
    /// </summary>
    public bool Autofocus
    {
        get => AutofocusAttribute.Value == "";
        set => AutofocusAttribute.Value = value ? "" : null;
    }

    public override void SetValueFromForm(string input)
        => Value = input;
    
    private static (string Type, string? Spellcheck, string? Autocomplete) ConfigurationFromRole(TextBoxRole role)
        => role switch
        {
            TextBoxRole.Normal => ("text", null, null),
            TextBoxRole.NoSpellcheck => ("text", "false", null),
            TextBoxRole.Username => ("text", "false", "username"),
            TextBoxRole.CurrentPassword => ("password", "false", "current-password"),
            TextBoxRole.NewPassword => ("password", "false", "new-password"),
            TextBoxRole.Email => ("email", "false", "email"),
            TextBoxRole.Phone => ("tel", "false", "tel"),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role")
        };
}