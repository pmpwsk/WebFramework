using uwap.WebFramework.Plugins;

namespace uwap.WebFramework.Elements;

/// <summary>
/// An IPage implementation with a default layout (navbar, sidebar, content, footer) and more options for most things a web page needs.
/// </summary>
public class Page : IPage
{
    /// <summary>
    /// The title of the page, an extension might be appended.
    /// </summary>
    public string Title;

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
    /// The list of buttons in the navigation bar.<br/>
    /// Default: empty list, a home button with the current domain will be added if it remains empty when exporting.
    /// </summary>
    public List<IButton> Navigation = [];

    /// <summary>
    /// The list of main elements (content).<br/>
    /// Default: empty list
    /// </summary>
    public List<IPageElement> Elements = [];

    /// <summary>
    /// The list of sidebar elements.<br/>
    /// Default: empty list
    /// </summary>
    public List<IPageElement> Sidebar = [];

    /// <summary>
    /// The list of things to preload.<br/>
    /// Default: empty list
    /// </summary>
    public List<Preload> Preloads = [];

    /// <summary>
    /// Whether to send a footer (=false) or not (=true).<br/>
    /// Default: false (sends a default footer with a copyright notice)
    /// </summary>
    public bool HideFooter = false;

    /// <summary>
    /// The JavaScript command the browser should execute when the HTML body is loaded or null to disable.<br/>
    /// Default: null
    /// </summary>
    public string? Onload = null;

    /// <summary>
    /// The page's description for search engines and link integrations or null to disable.<br/>
    /// Default: null
    /// </summary>
    public string? Description = null;

    /// <summary>
    /// The URL of the page's icon or null to disable.<br/>
    /// Default: null
    /// </summary>
    public string? Favicon = null;

    /// <summary>
    /// Custom lines to be added to the HTML head.<br/>
    /// Default: empty list
    /// </summary>
    public List<string> Head = [];

    /// <summary>
    /// Creates a new empty page with the given title.
    /// </summary>
    public Page(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Creates a new empty page with the given title and style.
    /// </summary>
    public Page(string title, IStyle style)
    {
        Title = title;
        Styles.Add(style);
    }

    /// <summary>
    /// Creates a new empty page with the given title and styles.
    /// </summary>
    public Page(string title, List<IStyle> styles)
    {
        Title = title;
        Styles = styles;
    }

    //documentation inherited from IPage
    public IEnumerable<string> Export(Request req)
        => Export(req, true);

    public IEnumerable<string> ExportWithoutCheckingForError(Request req)
        => Export(req, false);

    private IEnumerable<string> Export(Request req, bool checkForErrors)
    {
        bool error = req.Status != 200;

        if (error && checkForErrors)
        {
            if (!Server.Config.StatusMessages.TryGetValue(req.Status, out var message))
                message = $"{((req.Status < 400) ? "Status" : "Error")} {req.Status}";
            if (req.Status == 500 && req is { Exception: not null, IsAdmin: true })
                message += $"<br/>Type: {(req.Exception.GetType().FullName ?? "Unknown").HtmlSafe()}" +
                    $"<br/>Message: {req.Exception.Message.HtmlSafe()}" +
                    $"<br/>StackTrace: {(req.Exception.StackTrace??"Unknown").HtmlSafe()}";

            foreach (string domain in req.Domains)
            {
                if (Server.Cache.TryGetValue($"{domain}/status/{req.Status}.wfpg", out var entry)
                    || Server.Cache.TryGetValue($"{domain}/status/any.wfpg", out entry))
                {
                    foreach (string line in Server.ParseStatusPageAndReturnExport(req, entry, message))
                        yield return line;
                    goto end;
                }
            }
        }

        yield return "<!DOCTYPE html>";
        yield return "<html>";
        yield return "<head>";

        //title
        string title = (error && checkForErrors) ? ((req.Status == 301 || req.Status == 302) ? "Redirecting" : "Error") : Title.HtmlSafe();
        if (Server.Config.Domains.TitleExtensions.TryGetValueAny(out var titleExtension, req.Domains) && titleExtension != null)
            title += " | " + titleExtension;
        yield return $"\t<title>{title}</title>";

        //description
        if (Description != null)
            yield return $"\t<meta name=\"description\" content=\"{Description.HtmlValueSafe()}\" />";

        //canonical
        if (Server.Config.Domains.CanonicalDomains.TryGetValueAny(out var canonical, req.Domains))
        {
            canonical ??= req.Domain;
            yield return $"\t<link rel=\"canonical\" href=\"https://{canonical}{req.FullPath}{req.Query.FullString}\" />";
        }

        //viewport settings + charset
        yield return $"\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, interactive-widget=resizes-content\" />";
        yield return "\t<meta charset=\"utf-8\">";

        //favicon
        if (Favicon != null)
        {
            string? mime = Favicon;
            int slash = Favicon.LastIndexOf('/');
            if (slash != -1)
                mime = mime.Remove(0, slash + 1);
            int dot = mime.LastIndexOf('.');
            if (dot != -1)
            {
                mime = mime.Remove(0, dot);
                mime = Server.Config.MimeTypes.TryGetValue(mime, out mime) ? $" type=\"{mime}\"" : "";
            }

            Parsers.FormatPath(req, Favicon, req.Domains, out var faviconPath, out var faviconDomains, out var faviconQuery);
            if (Server.Cache.TryGetValueAny(out var faviconA, faviconDomains.Select(d => d + faviconPath).ToArray()) && faviconA.IsPublic)
                yield return $"\t<link rel=\"icon\"{mime} href=\"{(Favicon + Parsers.QueryStringSuffix(faviconQuery, $"t={faviconA.GetModifiedUtc().Ticks}")).HtmlValueSafe()}\">";
            else
            {
                IPlugin? plugin = PluginManager.GetPlugin(req, faviconDomains, faviconPath, out string relPath, out _, out _);
                if (plugin != null)
                {
                    string? timestamp = plugin.GetFileVersion(relPath);
                    if (timestamp != null)
                        yield return $"\t<link rel=\"icon\"{mime} href=\"{(Favicon + Parsers.QueryStringSuffix(faviconQuery, $"t={timestamp}")).HtmlValueSafe()}\">";
                    else yield return $"\t<link rel=\"icon\"{mime} href=\"{Favicon.HtmlValueSafe()}\">";
                }
                else yield return $"\t<link rel=\"icon\"{mime} href=\"{Favicon.HtmlValueSafe()}\">";
            }
        }

        //preloads
        foreach (Preload preload in Preloads)
            yield return "\t" + preload.Export();

        //styles
        foreach (IStyle style in Styles)
            foreach (string line in style.Export(req))
                yield return "\t" + line;

        //custom head items
        foreach (string line in Head)
            yield return $"\t{line}";

        yield return "</head>";
        yield return $"<body{((error&&checkForErrors)||Onload==null?"":$" onload=\"{Onload.HtmlValueSafe()}\"")}>";

        //navbar
        var nav = (Navigation.Count != 0) ? Navigation : [new Button(req.Domain, "/")];
        yield return "\t<div class=\"nav\">";
        foreach (IButton n in nav)
            yield return "\t\t" + n.Export();
        yield return "\t</div>";

        yield return "\t<div class=\"full\">";
        //sidebar
        yield return "\t\t<div class=\"sidebar\">";
        yield return "\t\t\t<div class=\"sidebar-items\">";
            foreach (IPageElement e in Sidebar)
                foreach (string line in e.Export())
                    yield return "\t\t\t\t" + line;
        yield return "\t\t\t</div>";
        yield return "\t\t</div>";

        //content
        yield return "\t\t<div class=\"content\">";
        yield return "\t\t\t<div class=\"content-items\">";
        if (req.LoginState == Accounts.LoginState.Banned && Server.Config.Accounts.FailedAttempts.BanMessage != null)
            foreach (string line in Server.Config.Accounts.FailedAttempts.BanMessage.Export())
                yield return "\t\t\t\t" + line;
        if (error && checkForErrors)
        {
            if (req.Status == 301 || req.Status == 302)
            {
                foreach (string line in new HeadingElement($"Redirecting...", "").Export())
                    yield return "\t\t\t\t" + line;
                foreach (string line in new ButtonElementJS("Try again", null, "window.location.reload()").Export())
                    yield return "\t\t\t\t" + line;
            }
            else
            {
                List<IContent> content;
                if (req is { Status: 500, Exception: not null, IsAdmin: true })
                    content =
                    [
                        new Paragraph((req.Exception.GetType().FullName??"Unknown").HtmlSafe()),
                        new Heading("Message"), new Paragraph(req.Exception.Message.HtmlSafe()),
                        new Heading("StackTrace"), new Paragraph((req.Exception.StackTrace??"Unknown").HtmlSafe())
                    ];
                else if (Server.Config.StatusMessages.TryGetValue(req.Status, out var statusMessage))
                    content = [new Paragraph(statusMessage)];
                else content = [];

                foreach (string line in new HeadingElement($"Error {req.Status}", content, "red").Export())
                    yield return "\t\t\t\t" + line;
            }
        }
        else foreach (IPageElement e in Elements)
            foreach (string line in e.Export())
                yield return "\t\t\t\t" + line;
        yield return "\t\t\t</div>";

        //footer
        if (!HideFooter)
        {
            if (!Server.Config.Domains.CopyrightNames.TryGetValueAny(out var copyright, req.Domains))
                copyright = req.Domain;
            List<IContent> footerContent = [];
            if (copyright != null)
                footerContent.Add(new Paragraph($"Copyright {DateTime.UtcNow.Year} {copyright} - All other trademarks, screenshots, logos and copyrights are the property of their respective owners."));
            footerContent.Add(new Paragraph("Powered by <a href=\"https://uwap.org/wf\">uwap.org/wf</a>") {Unsafe = true});
                
            foreach (string line in new ContainerElement(null, footerContent, "footer").Export())
                yield return "\t\t\t" + line;
        }
        yield return "\t\t</div>";
        yield return "\t</div>";

        //scripts
        if (!(error&&checkForErrors))
            foreach (IScript script in Scripts)
                foreach (string line in script.Export(req))
                    yield return "\t" + line;

        yield return "</body>";
        yield return "</html>";

    end:;
    }

    /// <summary>
    /// Fills the sidebar with page navigation buttons for every element with a title and ID.<br/>
    /// If addHeading is true, a heading "Navigation:" will be added.
    /// </summary>
    public void PopulateSidebar(bool addHeading)
    {
        if (addHeading)
            Sidebar.Add(new ContainerElement("Navigation:", ""));
        foreach (IPageElement element in Elements)
        {
            if (element.Id == null)
                continue;

            string? title = null;
            if (element is IContainerElement container)
                title = container.Title;
            else if (element is IButtonElement button)
                title = button.Title;

            if (title != null)
                Sidebar.Add(new ButtonElement(null, title.TrimEnd(':'), $"#{element.Id}"));
        }
    }
}