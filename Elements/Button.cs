namespace uwap.WebFramework.Elements;

public class Button : IButton
{
    protected override string? ElementProperties => $"href=\"{Link}\"" + (NewTab?" target=\"_blank\"":"") + (NoFollow?" rel=\"nofollow\"":"");

    public string Link;
    public bool NewTab;
    public bool NoFollow;

    public Button(string text, string link, string? classes = null, string? styles = null, string? id = null, bool newTab = false, bool noFollow = false)
    {
        Text = text;
        Link = link;
        Class = classes;
        Style = styles;
        Id = id;
        NewTab = newTab;
        NoFollow = noFollow;
    }
}