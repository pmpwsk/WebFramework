namespace uwap.WebFramework.Elements;

public class FlexibleTextArea : IContent
{
    protected override string ElementType => "span";
    protected override string? ElementClass => "textarea";
    protected override string? ElementProperties => $"role=\"textbox\" contenteditable{(Autofocus?" autofocus":"")}{(OnInput!=null?$" oninput=\"{OnInput}\"":"")}";

    public string? Text = null;
    public bool Autofocus;
    public string? OnInput = null;

    public FlexibleTextArea(string? text, string id, string? classes = null, string? styles = null, bool autofocus = false, string? onInput = null)
    {
        Text = text;
        Id = id;
        Class = classes;
        Style = styles;
        Autofocus = autofocus;
        OnInput = onInput;
    }

    public override ICollection<string> Export()
        => new List<string> { Opener + (Text??"") + Closer };
}