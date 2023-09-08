namespace uwap.WebFramework.Elements;

public class ButtonElementJS : IButtonElement
{
    protected override string? ElementProperties => $"href=\"javascript:\" onclick=\"{Command}\"";

    public string Command;

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