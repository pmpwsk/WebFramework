namespace uwap.WebFramework.Elements;

/// <summary>
/// Generic button for JavaScript commands.
/// </summary>
public class ButtonJS : IButton
{
    //documentation inherited from IElement
    protected override string? ElementProperties => $"href=\"javascript:\" onclick=\"{Command}\"";

    /// <summary>
    /// The command to execute.
    /// </summary>
    public string Command;

    /// <summary>
    /// Creates a new generic button for JavaScript commands.
    /// </summary>
    public ButtonJS(string text, string command, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Command = command;
        Class = classes;
        Style = styles;
        Id = id;
    }
}