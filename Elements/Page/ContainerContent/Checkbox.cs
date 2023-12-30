namespace uwap.WebFramework.Elements;

/// <summary>
/// Checkbox that can either be enabled or disabled.
/// </summary>
public class Checkbox : IContent
{
    //not needed
    protected override string ElementType => "";

    /// <summary>
    /// The text for the checkbox.
    /// </summary>
    public string Text;

    /// <summary>
    /// Whether the checkbox should be checked by default.
    /// </summary>
    public bool Checked;

    /// <summary>
    /// JavaScript command that should be executed when the value changes.
    /// </summary>
    public string? OnChange = null;

    /// <summary>
    /// Creates a new checkbox element with the given ID for a container.
    /// </summary>
    public Checkbox(string text, string id, bool isChecked = false)
    {
        Text = text;
        Id = id;
        Checked = isChecked;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return $"<div class=\"checkbox\"><input type=\"checkbox\" id=\"{Id}\" name=\"{Id}\"{(Checked ? " checked" : "")}{(OnChange == null ? null : $" onchange=\"{OnChange.HtmlValueSafe()}\"")} /><label for=\"{Id}\">{Text}</label></div>";
    }
}