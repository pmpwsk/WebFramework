using uwap.WebFramework.Elements;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// Attempts to handle the given request using a .wfpg file in ../Public for any of the domains (in order) and returns true if that was possible. If no matching file was found, false is returned.
    /// </summary>
    private static bool ParsePage(AppRequest request, List<string> domains)
    {
        string path = request.Path;
        if (path.EndsWith("/index"))
            return false;
        if (path.EndsWith('/'))
            path += "index";
        path += ".wfpg";
        foreach (string domain in domains)
        {
            if (Cache.TryGetValue(domain + path, out CacheEntry? entry) && entry.IsPublic)
            {
                ParsePage(request, entry);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Creates a blank page and parses the .wfpg file in the given cache entry to populate it with data and elements.
    /// </summary>
    private static void ParsePage(AppRequest request, CacheEntry cacheEntry)
    {
        Presets.CreatePage(request, cacheEntry.Key.After('/').RemoveLast(5).CapitalizeFirstLetter(), out Page page, out _);
        Presets.Navigation(request, page);
        ParseIntoPage(request, page, cacheEntry.EnumerateTextLines());
    }

    /// <summary>
    /// Parses the .wfpg file in the given cache entry and populates the given page with it.
    /// </summary>
    public static void ParseIntoPage(AppRequest req, Page page, IEnumerable<string> lines)
    {
        IPageElement? currentContentElement = null;
        IPageElement? currentSidebarElement = null;

        foreach (string lineUntrimmed in lines)
        {
            string line = lineUntrimmed.Trim();
            if (line == "")
            {
                currentContentElement = null;
                currentSidebarElement = null;
            }
            else if (line.StartsWith('#'))
            { }
            else if (line.StartsWith(">>"))
                ParseCommand(req, page, line.Remove(0, 2).TrimStart());
            else if (line.StartsWith("<<"))
            {
                line = line.Remove(0, 2).TrimStart();
                IPageElement? newElement = ParseElement(page.Sidebar, currentSidebarElement, line, true, page);
                if (newElement != null)
                {
                    currentSidebarElement = newElement;
                    page.Sidebar.Add(newElement);
                }
            }
            else if (line.StartsWith("^^") && ParseSpecialElement(line.Remove(0, 2).TrimStart(), out string p1, out string? p2, out string target))
            {
                IButton button;
                if (target.StartsWithAny("/", "http://", "https://"))
                    button = new Button(p1, target, p2, newTab: !target.StartsWithAny("/", "#"), noFollow: !target.StartsWithAny("/", "#"));
                else button = new ButtonJS(p1, target.Replace('"', '\''), p2);
                page.Navigation.Add(button);
            }
            else
            {
                IPageElement? newElement = ParseElement(page.Elements, currentContentElement, line, false, page);
                if (newElement != null)
                {
                    currentContentElement = newElement;
                    page.Elements.Add(newElement);
                }
            }
        }
        
        if (page.Description == "")
            page.Description = null;
    }

    /// <summary>
    /// Parses and applies the command to the page.
    /// </summary>
    private static void ParseCommand(AppRequest req, Page page, string command)
    {
        command.SplitAtFirst(' ', out string operation, out string arguments);
        operation = operation.Trim();
        arguments = arguments.Trim();
        switch (operation)
        {
            case "title":
            case "t":
                page.Title = arguments;
                break;
            case "description":
            case "d":
                page.Description = arguments;
                break;
            case "import":
            case "i":
                if (Cache.TryGetValue($"{arguments}.wfpg", out CacheEntry? cacheEntry))
                    ParseIntoPage(req, page, cacheEntry.EnumerateTextLines());
                break;
            case "script":
                if (arguments == "")
                    page.Styles.Clear();
                else page.Scripts.Add(new Script(arguments));
                break;
            case "style":
                if (arguments == "")
                    page.Styles.Clear();
                else page.Styles.Add(new Style(arguments));
                break;
            case "sidebar":
            case "s":
                arguments.SplitAtLast('|', out string arg1, out string arg2);
                arg1 = arg1.TrimEnd();
                arg2 = arg2.TrimStart();
                switch (arg1)
                {
                    case "":
                        page.Sidebar.Clear();
                        break;
                    case "populate":
                        page.PopulateSidebar(true);
                        break;
                    case "fill":
                        page.PopulateSidebar(false);
                        break;
                    case "highlight":
                        foreach (IPageElement element in page.Sidebar)
                            if (element is ButtonElement button && button.Link == req.Path + req.Context.Query())
                            {
                                if (arg2 == "")
                                    button.Class = "green";
                                else button.Class = arg2;
                            }
                        break;
                }
                break;
            case "redirect":
            case "r":
                if (arguments != "" && !arguments.Contains("/api/"))
                {
                    req.Redirect(arguments);
                    return;
                }
                break;
            case "nav":
            case "n":
                if (arguments == "")
                    page.Navigation.Clear();
                break;
        }    
    }

    /// <summary>
    /// Parses the line and adds the element to the page.
    /// </summary>
    private static IPageElement? ParseElement(List<IPageElement> e, IPageElement? currentElement, string line, bool sidebar, Page page)
    {
        IPageElement? result = null;
        if (currentElement != null && currentElement is ContainerElement container)
        {
            if (line.StartsWith('-'))
            {
                line = line.Remove(0, 1).Trim();
                if (container.Contents.Count != 0 && container.Contents.Last() is BulletList bullets)
                    bullets.List.Add(line);
                else container.Contents.Add(new BulletList(line));
            }
            else if (ParseSpecialElement(line, out string p1, out string? p2, out string target))
            {
                if (target.StartsWithAny("/", "#", "http://", "https://"))
                {
                    if (target.EndsWithAny(".jpg", ".jpeg", ".png", ".gif", ".bmp"))
                        container.Contents.Add(new Image(target, "max-height: " + p1, title: p2));
                    else container.Buttons.Add(new Button(p1, target, p2, newTab: !target.StartsWithAny("/", "#"), noFollow: !target.StartsWithAny("/", "#")));
                }
                else container.Buttons.Add(new ButtonJS(p1, target.Replace('"', '\''), p2));
            }
            else
            {
                string text = line.Trim();
                if (page.Description == "")
                    page.Description = text;
                if (((text.StartsWith("<br/>") && text != "<br/>") || (text.StartsWith("<br />") && text != "<br />")) && container.Contents.Count > 0 && container.Contents.Last() is Paragraph p)
                    p.Text += text;
                else container.Contents.Add(new Paragraph(text));
            }
        }
        else if (currentElement != null && currentElement is IButtonElement buttonElement && buttonElement.Text == null)
            buttonElement.Text = line.Trim();
        else if (ParseSpecialElement(line, out string p1, out string? p2, out string? target))
        {
            string? titleLg = sidebar ? null : (p1 == "" ? null : p1);
            string? titleSm = sidebar ? (p1 == "" ? null : p1) : null;
            if (target.StartsWithAny("/", "#", "http://", "https://"))
            {
                if (target.EndsWithAny(".jpg", ".jpeg", ".png", ".gif", ".bmp"))
                {
                    if (p2 == null)
                        result = new ContainerElement(null, new Image(target, "max-height: " + p1));
                    else
                    {
                        string? title = ParseTitle(p2, out string? classes, out string? id);
                        result = new ContainerElement(sidebar ? null : title, new Image(target, "max-height: " + p1, title: title), classes, id: sidebar ? null : id);
                    }
                }
                else result = new ButtonElement(titleLg, titleSm, target, p2, newTab: !target.StartsWithAny("/", "#"), noFollow: !target.StartsWithAny("/", "#"), id: sidebar ? null : p1.ToId());
            }
            else result = new ButtonElementJS(titleLg, titleSm, target.Replace('"', '\''), p2, id: sidebar ? null : p1.ToId());
        }
        else if (e.Count == 0)
        {
            string? text = ParseTitle(line.Trim(), out string? classes, out string? id);
            if (sidebar)
                result = new ContainerElement(text, "", classes, id: sidebar ? null : id);
            else result = new LargeContainerElement(text, "", classes, id: sidebar ? null : id);
        }
        else
        {
            string? text = ParseTitle(line.Trim(), out string? classes, out string? id);
            if (sidebar)
                result = new ContainerElement(null, text ?? "", classes, id: sidebar ? null : id);
            else result = new ContainerElement(text, "", classes, id: sidebar ? null : id);
        }
        return result;
    }

    /// <summary>
    /// Parses a line as a special element and returns true if it is indeed special, otherwise false.
    /// </summary>
    private static bool ParseSpecialElement(string line, out string p1, out string? p2, out string target)
    {
        if (line.SplitAtLast(">>", out p1, out target))
        {
            p1.SplitAtLast('|', out p1, out p2);
            p1 = p1.Trim();
            target = target.Trim();
            p2 = p2.Trim();
            if (p2 == "") p2 = null;
            return true;
        }
        else
        {
            p2 = null;
            return false;
        }
    }

    /// <summary>
    /// Parses a line as a title along with a class, if separated with |.
    /// </summary>
    private static string? ParseTitle(string line, out string? classes, out string? id)
    {
        line.SplitAtLast('|', out string? title, out classes);
        title = title.Trim();
        if (title == "")
            title = null;
        classes = classes.Trim();
        if (classes == "") classes = null;
        id = title?.ToId();
        return title;
    }
}