namespace uwap.WebFramework.Elements;

public enum TextBoxRole
{
    None,
    NoSpellcheck,
    Username,
    Email,
    Password,
    NewPassword,
    Phone
}

public class TextBox : IContent
{
    protected override string ElementType => "input";
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

    public string? Text;
    public string? Placeholder;
    public TextBoxRole Role;
    public string? OnEnter, OnInput;
    public bool Autofocus;

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

    public override ICollection<string> Export()
    {
        if (OnEnter != null)
        {
            return new List<string> { $"<form action=\"javascript:{OnEnter}\">", "\t"+Opener, "\t<input type=\"submit\" style=\"visibility:hidden;width:0;height:0\"/>", "</form>"};
        }
        else return new List<string> { Opener };
    }
}