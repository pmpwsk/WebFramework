namespace uwap.WebFramework.Elements;

public class TextArea : IContent
{
    protected override string ElementType => "textarea";
    protected override string? ElementProperties
    {
        get
        {
            List<string> properties = new List<string>();
            if (Placeholder != "") properties.Add($"placeholder=\"{Placeholder}\"");
            if (Rows != null) properties.Add($"rows=\"{Rows}\"");
            if (Autofocus) properties.Add("autofocus");
            if (OnInput != null) properties.Add($"oninput=\"{OnInput}\"");
            return string.Join(' ', properties);
        }
    }

    public string? Text = null;
    public string Placeholder;
    public int? Rows = null;
    public bool Autofocus = false;
    public string? OnInput = null;

    public TextArea(string placeholder, string? text, string id, int? rows = null, string? classes = null, string? styles = null, bool autofocus = false, string? onInput = null)
    {
        Placeholder = placeholder;
        Text = text;
        Id = id;
        Rows = rows;
        Class = classes;
        Style = styles;
        Autofocus = autofocus;
        OnInput = onInput;
    }

    public override ICollection<string> Export()
        => new List<string> { Opener + (Text??"") + Closer };
}