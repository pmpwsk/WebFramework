namespace uwap.WebFramework.Elements;

public class ButtonElement : IButtonElement
{
    protected override string? ElementProperties => $"href=\"{Link}\"" + (NewTab?" target=\"_blank\"":"") + (NoFollow?" rel=\"nofollow\"":"");

    public string Link;
    public bool NewTab;
    public bool NoFollow;

    public ButtonElement(string? title, string? text, string link, string? classes = null, string? styles = null, string? id = null, bool newTab = false, bool noFollow = false)
    {
        Title = title;
        Text = text;
        Link = link;
        Class = classes;
        Style = styles;
        Id = id;
        NewTab = newTab;
        NoFollow = noFollow;
    }
}