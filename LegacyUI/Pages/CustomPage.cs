namespace uwap.WebFramework.Elements;

/// <summary>
/// An IPage with custom lines for head and body as well as a title string and lists for styles and scripts.
/// </summary>
public class CustomPage(string title) : IPage
{
    /// <summary>
    /// The title of the page, an extension might be appended.
    /// </summary>
    public string Title = title;

    /// <summary>
    /// The list of styles that will be sent.<br/>
    /// Default: empty list
    /// </summary>
    public List<IStyle> Styles = [];

    /// <summary>
    /// The list of scripts that will be sent.<br/>
    /// Default: empty list
    /// </summary>
    public List<IScript> Scripts = [];

    /// <summary>
    /// The lines of the HTML head that will be sent.<br/>
    /// Default: empty list
    /// </summary>
    public List<string> HeadLines = [];

    /// <summary>
    /// The lines of the HTML head that will be sent.<br/>
    /// Default: empty list
    /// </summary>
    public List<string> BodyLines = [];

    //documentation inherited from IPage
    public IEnumerable<string> Export(Request req)
    {
        yield return "<!DOCTYPE html>";
        yield return "<html>";

        ///head
        yield return "<head>";
        //title
        string title = Title.HtmlSafe();
        if (Server.Config.Domains.TitleExtensions.TryGetValueAny(out var titleExtension, req.Domains) && titleExtension != null)
            title += " | " + titleExtension;
        yield return $"\t<title>{title}</title>";
        //viewport settings + charset
        yield return $"\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, interactive-widget=resizes-content\" />";
        yield return "\t<meta charset=\"utf-8\">";
        //styles
        foreach (IStyle style in Styles)
            foreach (string line in style.Export(req))
                yield return "\t" + line;
        foreach (string item in HeadLines)
            yield return "\t" + item;
        yield return "</head>";

        ///body
        yield return "<body>";
        foreach (string item in BodyLines)
            yield return "\t" + item;
        foreach (IScript script in Scripts)
            foreach (string line in script.Export(req))
                yield return "\t" + line;
        yield return "</body>";

        yield return "</html>";
    }
}