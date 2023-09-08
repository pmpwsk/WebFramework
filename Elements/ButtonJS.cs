namespace uwap.WebFramework.Elements;

public class ButtonJS : IButton
{
    protected override string? ElementProperties => $"href=\"javascript:\" onclick=\"{Command}\"";

    public string Command;

    public ButtonJS(string text, string command, string? classes = null, string? styles = null, string? id = null)
    {
        Text = text;
        Command = command;
        Class = classes;
        Style = styles;
        Id = id;
    }
}