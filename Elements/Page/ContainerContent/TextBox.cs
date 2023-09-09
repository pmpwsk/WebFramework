namespace uwap.WebFramework.Elements;

/// <summary>
/// Possible roles for text input boxes.
/// </summary>
public enum TextBoxRole
{
    /// <summary>
    /// No particular role.
    /// </summary>
    None,

    /// <summary>
    /// Disables spell checking by the user's system.
    /// </summary>
    NoSpellcheck,

    /// <summary>
    /// Suggests autofilling a saved username.
    /// </summary>
    Username,

    /// <summary>
    /// Suggests autofilling a saved email address.
    /// </summary>
    Email,

    /// <summary>
    /// Suggests autofilling a saved password.
    /// </summary>
    Password,

    /// <summary>
    /// Suggests saving the entered password in the browser and maybe the generation of a new random password.
    /// </summary>
    NewPassword,

    /// <summary>
    /// Suggests autofilling of a saved phone number.
    /// </summary>
    Phone
}

/// <summary>
/// Text input box for a container.
/// </summary>
public class TextBox : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "input";

    //documentation inherited from IElement
    protected override string? ElementProperties
    {
        get
        {
            List<string> properties = new List<string>();
            if (Placeholder != null) properties.Add($"placeholder=\"{Placeholder}\"");
            properties.Add(Role switch
            {
                TextBoxRole.Username => "type=\"text\" spellcheck=\"false\" autocomplete=\"username\"",
                TextBoxRole.Email => $"type=\"email\" spellcheck=\"false\" autocomplete=\"email\"",
                TextBoxRole.Password => $"type=\"password\" spellcheck=\"false\" autocomplete=\"current-password\"",
                TextBoxRole.NewPassword => $"type=\"password\" spellcheck=\"false\" autocomplete=\"new-password\"",
                TextBoxRole.NoSpellcheck => $"type=\"text\" spellcheck=\"false\"",
                TextBoxRole.Phone => $"type=\"tel\" spellcheck=\"false\" autocomplete=\"tel\"",
                TextBoxRole.None or _ => $"type=\"text\""
            });
            if (Text != null) properties.Add($"value=\"{Text}\"");
            //if (OnEnter != null) properties.Add($"onkeydown=\"if (event.key == 'Enter') {OnEnter}\"");
            if (Autofocus) properties.Add("autofocus");
            if (OnInput != null) properties.Add($"oninput=\"{OnInput}\"");
            return string.Join(' ', properties);
        }
    }

    /// <summary>
    /// The text that is in the text area by default or null to disable.
    /// </summary>
    public string? Text;

    /// <summary>
    /// The placeholder that appears when the text area is empty or null to disable.
    /// </summary>
    public string? Placeholder;

    /// <summary>
    /// The role of this text box.
    /// </summary>
    public TextBoxRole Role;

    /// <summary>
    /// JavaScript command that should be executed when the enter key is pressed.
    /// </summary>
    public string? OnEnter;

    /// <summary>
    /// JavaScript command that should be executed when the text changes.
    /// </summary>
    public string? OnInput;

    /// <summary>
    /// Whether to automatically focus on this text area when the page loads.
    /// </summary>
    public bool Autofocus;

    /// <summary>
    /// Creates a new input text box for a container.
    /// </summary>
    public TextBox(string? placeholder, string? text, string id, TextBoxRole role = TextBoxRole.None, string? onEnter = null, string? classes = null, string? styles = null, bool autofocus = false, string? onInput = null)
    {
        Placeholder = placeholder;
        Text = text;
        Id = id;
        Role = role;
        OnEnter = onEnter;
        Class = classes;
        Style = styles;
        Autofocus = autofocus;
        OnInput = onInput;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        if (OnEnter != null)
        {
            yield return $"<form action=\"javascript:{OnEnter}\">";
            yield return "\t" + Opener;
            yield return "\t<input type=\"submit\" style=\"visibility:hidden;width:0;height:0\"/>";
            yield return "</form>";
        }
        else yield return Opener;
    }
}