using Microsoft.Extensions.Configuration;
using static QRCoder.PayloadGenerator;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework.Elements;

public class Page : IPage
{
    public List<IButton> Navigation = new();
    public List<IPageElement> Elements = new();
    public List<IPageElement> Sidebar = new();
    public List<Preload> Preloads = new();
    public bool HideFooter = false;
    public string? Onload = null;
    public string? Description = null;
    public string? Favicon = "/favicon.ico";
    public List<string> Head = new();

    public Page(string title)
    {
        Title = title;
    }

    public Page(string title, IStyle style)
    {
        Title = title;
        Styles.Add(style);
    }

    public Page(string title, List<IStyle> styles)
    {
        Title = title;
        if (styles != null) Styles = styles;
    }

    public override string Export(AppRequest request)
    {
        var domains = Parsers.Domains(request.Domain).ToArray();

        bool error = request.Status != 200;

        string page = "";
        page += "<!DOCTYPE html>";
        page += "\n<html>";
        page += "\n<head>";

        //title
        if (error)
        {
            if (request.Status == 301 || request.Status == 302)
                page += "\n\t<title>Redirecting</title>";
            else page += "\n\t<title>Error</title>";
        }
        else
        {
            string title = Title;
            if (Server.Config.Domains.TitleExtensions.TryGetValueAny(out var titleExtension, domains))
                title += " | " + titleExtension;
            page += $"\n\t<title>{title}</title>";
        }

        //description
        if (Description != null)
            page += $"\n\t<meta name=\"description\" content=\"{Description}\" />";

        //canonical
        if (Server.Config.Domains.CanonicalDomains.TryGetValueAny(out var canonical, domains))
        {
            canonical ??= request.Domain;
            page += $"\n\t<link rel=\"canonical\" href=\"https://{canonical}{request.Path}{request.Context.Request.QueryString}\" />";
        }

        //viewport settings + charset
        page += $"\n\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />";
        page += "\n\t<meta charset=\"utf-8\">";

        //favicon
        if (Favicon != null)
        {
            string? mime = Favicon;
            int slash = Favicon.LastIndexOf('/');
            if (slash != -1)
            {
                mime = mime.Remove(0, slash + 1);
            }
            int dot = mime.LastIndexOf('.');
            if (dot != -1)
            {
                mime = mime.Remove(0, dot);
                if (Server.Config.MimeTypes.TryGetValue(mime, out mime))
                    mime = $" type=\"{mime}\"";
                else mime = "";
            }

            if (Server.Cache.TryGetValueAny(out var faviconA, domains.Select(d => d + Favicon).ToArray()))
            {
                page += $"\n\t<link rel=\"icon\"{mime} href=\"{Favicon}?t={faviconA.GetModifiedUtc().Ticks}\">";
            }
            else
            {
                IPlugin? plugin = PluginManager.GetPlugin(domains, Favicon, out string relPath, out _);
                if (plugin != null)
                {
                    string? timestamp = plugin.GetFileVersion(relPath);
                    if (timestamp != null)
                        page += $"\n\t<link rel=\"icon\"{mime} href=\"{Favicon}?t={timestamp}\">";
                    else page += $"\n\t<link rel=\"icon\"{mime} href=\"{Favicon}\">";
                }
                else page += $"\n\t<link rel=\"icon\"{mime} href=\"{Favicon}\">";
            }
        }

        //preloads
        foreach (Preload preload in Preloads)
            page += "\n\t" + preload.Export();

        //styles
        foreach (IStyle style in Styles)
            foreach (string line in style.Export(request))
                page += "\n\t" + line;

        //custom head items
        foreach (string line in Head)
            page += $"\n\t{line}";

        page += "\n</head>";
        page += $"\n<body{(error||Onload==null?"":$" onload=\"{Onload}\"")}>";

        //navbar
        var nav = (Navigation.Count != 0) ? Navigation : new List<IButton> { new Button(request.Domain, "/") };
        page += "\n\t<div class=\"nav\">";
        foreach (IButton n in nav)
            page += "\n\t\t" + n.Export();
        page += "\n\t</div>";

        page += "\n\t<div class=\"full\">";
        //sidebar
        page += "\n\t\t<div class=\"sidebar\">";
        page += "\n\t\t\t<div class=\"sidebar-items\">";
            foreach (IPageElement e in Sidebar)
                foreach (string line in e.Export())
                    page += "\n\t\t\t\t" + line;
        page += "\n\t\t\t</div>";
        page += "\n\t\t</div>";

        //content
        page += "\n\t\t<div class=\"content\">";
        page += "\n\t\t\t<div class=\"content-items\">";
        if (error)
        {
            if (request.Status == 301 || request.Status == 302)
            {
                foreach (string line in new HeadingElement($"Redirecting...", "").Export())
                    page += "\n\t\t\t\t" + line;
                foreach (string line in new ButtonElementJS("Try again", null, "window.location.reload()").Export())
                    page += "\n\t\t\t\t" + line;
            }
            else
            {
                List<IContent> content;
                if (request.Status == 500 && request.Exception != null && request.IsAdmin())
                {
                    content = new List<IContent>
                {
                    new Paragraph((request.Exception.GetType().FullName??"Unknown").HtmlSafe()),
                    new Heading("Message"), new Paragraph(request.Exception.Message.HtmlSafe()),
                    new Heading("StackTrace"), new Paragraph((request.Exception.StackTrace??"Unknown").HtmlSafe())
                };
                }
                else if (Server.Config.StatusMessages.TryGetValue(request.Status, out var statusMessage))
                    content = new List<IContent> { new Paragraph(statusMessage) };
                else content = new List<IContent>();

                foreach (string line in new HeadingElement($"Error {request.Status}", content, "red").Export())
                    page += "\n\t\t\t\t" + line;
            }
        }
        else
        {
            foreach (IPageElement e in Elements)
                foreach (string line in e.Export())
                    page += "\n\t\t\t\t" + line;
        }
        page += "\n\t\t\t</div>";

        //footer
        if (!HideFooter)
        {
            if (!Server.Config.Domains.CopyrightNames.TryGetValueAny(out var copyright, domains))
                copyright = request.Domain;
            List<IContent> footerContent = new();
            if (copyright != null)
            {
                footerContent.Add(new Paragraph($"Copyright {DateTime.UtcNow.Year} {copyright} - All other trademarks, screenshots, logos and copyrights are the property of their respective owners."));
            }
            footerContent.Add(new Paragraph("Powered by <a href=\"https://uwap.org/wf\">uwap.org/wf</a>"));
                
            foreach (string line in new ContainerElement(null, footerContent, "footer").Export())
            {
                page += "\n\t\t\t" + line;
            }
        }
        page += "\n\t\t</div>";
        page += "\n\t</div>";

        //scripts
        if (!error)
            foreach (IScript script in Scripts)
                foreach (string line in script.Export(request))
                    page += "\n\t" + line;

        page += "\n</body>";
        page += "\n</html>";
        return page;
    }

    public void PopulateSidebar(bool addHeading)
    {
        if (addHeading) Sidebar.Add(new ContainerElement("Navigation:", ""));
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