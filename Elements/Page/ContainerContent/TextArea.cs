using System.Text;

namespace uwap.WebFramework.Elements;

public class TextArea : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "textarea";

    //documentation inherited from IElement
    protected override string? ElementProperties
    {
        get
        {
            List<string> properties = new List<string>();
            if (Placeholder != "" && Placeholder != null) properties.Add($"placeholder=\"{Placeholder}\"");
            if (Rows != null) properties.Add($"rows=\"{Rows}\"");
            if (Autofocus) properties.Add("autofocus");
            if (OnInput != null) properties.Add($"oninput=\"{OnInput}\"");
            return string.Join(' ', properties);
        }
    }

    /// <summary>
    /// The text that is in the text area by default or null to disable.
    /// </summary>
    public string? Text = null;

    /// <summary>
    /// The placeholder that appears when the text area is empty or null to disable.
    /// </summary>
    public string? Placeholder;

    /// <summary>
    /// The amount of rows to display (decides on the height of the text area) or null to not specify a row count.
    /// </summary>
    public int? Rows = null;

    /// <summary>
    /// Whether to automatically focus on this text area when the page loads.
    /// </summary>
    public bool Autofocus = false;

    /// <summary>
    /// JavaScript command that should be executed when the text changes.
    /// </summary>
    public string? OnInput = null;

    /// <summary>
    /// Creates a new text area for a container.
    /// </summary>
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

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        StringBuilder text = new();
        foreach (char c in Text ?? "")
            switch (c)
            {
                case '\n':
                    text.Append("&#13;&#10;");
                    break;
                case '<':
                    text.Append("&lt;");
                    break;
                case '>':
                    text.Append("&gt;");
                    break;
                default:
                    text.Append(c);
                    break;
            }
        yield return Opener + text.ToString() + Closer;
    }
}