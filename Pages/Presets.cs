using uwap.WebFramework.Accounts;
using uwap.WebFramework.Mail;

namespace uwap.WebFramework.Elements;

/// <summary>
/// Manages presets for pages so plugins and .wfpg files can follow the same page layout and design as your own stuff.
/// </summary>
public static class Presets
{
    /// <summary>
    /// The preset handler that decides everything.
    /// </summary>
    public static PresetsHandler Handler { get; set; } = new();

    /// <summary>
    /// Sends an important email to the given user using the given subject and text. A different address may be specified.
    /// </summary>
    public static MailSendResult WarningMail(Request req, User user, string subject, string text, string? useThisAddress = null)
        => Handler.WarningMail(req, user, subject, text, useThisAddress);

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns it.
    /// </summary>
    public static Page CreatePage(Request req, string title)
    {
        Page page = Handler.CreatePage(req, title);
        page.Favicon = Handler.Favicon(req);
        page.Styles = Handler.Styles(req, out var fontUrl);
        if (Handler.PreloadFont)
            page.Preloads.Add(new Preload(fontUrl, "font"));
        Navigation(req, page);
        req.Page = page;
        return page;
    }
    
    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns as an out parameter.
    /// </summary>
    public static void CreatePage(Request req, string title, out Page page)
    {
        page = CreatePage(req, title);
    }

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns it and its list of elements as an out parameter for easy access.
    /// </summary>
    public static void CreatePage(Request req, string title, out Page page, out List<IPageElement> e)
    {
        page = CreatePage(req, title);
        e = page.Elements;
    }

    /// <summary>
    /// Populates the navigation bar of the given page using information from the given request.
    /// </summary>
    public static void Navigation(Request req, Page page)
        => Handler.Navigation(req, page);

    /// <summary>
    /// Returns a list of styles that should be used for the given request as well as the URL of the used font in order to preload this if desired.
    /// </summary>
    public static List<IStyle> Styles(Request req, out string fontUrl)
        => Handler.Styles(req, out fontUrl);

    /// <summary>
    /// Assumes that the request already has a Page (not IPage!) object and returns the page object as well as the list of elements for easy access.
    /// </summary>
    public static void Init(this Request req, out Page page, out List<IPageElement> elements)
    {
        if (req.Page == null)
            throw new Exception("No page was set.");

        page = (Page)req.Page;
        elements = page.Elements;
    }

    /// <summary>
    /// Adds the script at /scripts[PATH].js to the Page to the given request (assuming it has one).
    /// </summary>
    public static void AddScript(this Request req)
    {
        if (req.Page != null)
            ((Page)req.Page).AddScript(req.Path);
    }

    /// <summary>
    /// Adds the script at /scripts[PATH]-[SUFFIX].js to the Page to the given request (assuming it has one).
    /// </summary>
    public static void AddScript(this Request req, string suffix)
    {
        if (req.Page != null)
            ((Page)req.Page).AddScript(req.Path + "-" + suffix);
    }

    /// <summary>
    /// Adds the script at /scripts[URL-MIDDLE].js to the given Page.
    /// </summary>
    public static void AddScript(this Page page, string urlMiddle)
        => page.Scripts.Add(new Script("/scripts" + urlMiddle + ".js"));

    /// <summary>
    /// Adds the default error script and the default error element (below the last current element or at the top if empty) to the page.
    /// </summary>
    public static void AddError(this Page page)
    {
        page.Scripts.Add(ErrorScript);
        page.Elements.Add(ErrorElement);
    }

    /// <summary>
    /// Sets the title of the page and adds a HeadingElement with the given subtext to it.
    /// </summary>
    public static void AddTitle(this Page page, string title, string text = "")
    {
        page.Title = title;
        page.Elements.Add(new HeadingElement(title, text));
    }

    /// <summary>
    /// Adds a button to contact customer support to the page.
    /// </summary>
    public static void AddSupportButton(this Page page, Request req)
        => Handler.AddSupportButton(req, page);

    /// <summary>
    /// The default error script.
    /// </summary>
    public static IScript ErrorScript
        => new CustomScript("let error = document.querySelector(\"#error\");\n\nfunction ShowError(message) {\n\terror.firstElementChild.innerText = message;\n\terror.style.display = \"block\";\n}\n\nfunction HideError() {\n\terror.style.display = \"none\";\n}");
    
    /// <summary>
    /// The default redirect script.
    /// </summary>
    public static IScript RedirectScript
        => new CustomScript("function Redirect() {\n\ttry {\n\t\tlet query = new URLSearchParams(window.location.search);\n\t\tif (query.has(\"redirect\"))\n\t\t{\n\t\t\tlet redirect = query.get(\"redirect\");\n\t\t\tif (redirect.startsWith(\"/\") || redirect.startsWith(\"https://\") || redirect.startsWith(\"http://\"))\n\t\t\t\twindow.location.assign(redirect);\n\t\t\telse window.location.assign(\"/\");\n\t\t}\n\t\telse window.location.assign(\"/\");\n\t} catch {\n\t\twindow.location.assign(\"/\");\n\t}\n}");

    /// <summary>
    /// The default error element.
    /// </summary>
    public static ButtonElementJS ErrorElement
        => new(null, "Error", "HideError()", "red", "display: none;", "error");

    /// <summary>
    /// Returns an account navbar button (to log in or access the logged in account).
    /// </summary>
    public static IButton AccountButton(Request req)
        => Handler.AccountButton(req);

    /// <summary>
    /// Adds elements to allow for password (and 2FA if present) verification with input IDs 'password' and 'code'.
    /// </summary>
    public static void AddAuthElements(this Request req)
        => Handler.AddAuthElements(req);

    /// <summary>
    /// Checks whether the given password (and 2FA code if necessary) provided in the query (keys 'password' and 'code') is correct for the given user.<br/>
    /// If it is correct, true is returned and nothing else happens.
    /// If it is incorrect, false is returned and "no" is written and the user is reported for failed authentication.
    /// </summary>
    public static async Task<bool> Auth(this Request req, User user)
    {
        if (req.Query.ContainsKey("password") && req.Query.ContainsKey("code"))
        {
            string password = req.Query["password"], code = req.Query["code"];
            if (user.ValidatePassword(password, null))
            {
                if (user.TwoFactor.TOTPEnabled(out var totp) && !totp.Validate(code, req, true))
                {
                    AccountManager.ReportFailedAuth(req.Context);
                    await req.Write("no");
                    return false;
                }
                else return true;
            }
            else
            {
                AccountManager.ReportFailedAuth(req.Context);
                await req.Write("no");
                return false;
            }
        }
        else
        {
            req.Status = 400;
            return false;
        }
    }

    /// <summary>
    /// The path of the login page.<br/>
    /// Default: [Handler.UsersPluginPath]/login
    /// </summary>
    public static string LoginPath(Request req)
        => Handler.LoginPath(req);
}
