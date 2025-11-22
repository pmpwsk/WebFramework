namespace uwap.WebFramework.Elements;

/// <summary>
/// Generic button for hyperlinks.
/// </summary>
public class Button : IButton
{
    //documentation inherited from IElement
    protected override string? ElementProperties => $"href=\"{Link.HtmlValueSafe()}\"" + (NewTab?" target=\"_blank\"":"") + (NoFollow?" rel=\"nofollow\"":"");

    /// <summary>
    /// The target URL.
    /// </summary>
    public string Link;

    /// <summary>
    /// Whether to open the link in a new tab.
    /// </summary>
    public bool NewTab;

    /// <summary>
    /// Whether to ask crawling bots to not follow the link.
    /// </summary>
    public bool NoFollow;

    /// <summary>
    /// Creates a new generic button for hyperlinks.
    /// </summary>
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