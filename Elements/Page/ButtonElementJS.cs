namespace uwap.WebFramework.Elements;

/// <summary>
/// Button element for JavaScript commands.
/// </summary>
public class ButtonElementJS : IButtonElement
{
    //documentation inherited from IElement
    protected override string? ElementProperties => $"href=\"javascript:\" onclick=\"{Command}\"";

    /// <summary>
    /// The command to execute.
    /// </summary>
    public string Command;

    /// <summary>
    /// Creates a new button element for JavaScript commands.
    /// </summary>
    public ButtonElementJS(string? title, string? text, string command, string? classes = null, string? styles = null, string? id = null)
    {
        Title = title;
        Text = text;
        Command = command;
        Class = classes;
        Style = styles;
        Id = id;
    }
}