using uwap.WebFramework.Accounts;
using uwap.WebFramework.Mail;
using uwap.WebFramework.Responses;
using uwap.WebFramework.Responses.DefaultUI;

namespace uwap.WebFramework;

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
    public static Task<MailSendResult> WarningMailAsync(Request req, User user, string subject, string text, string? useThisAddress = null)
        => Handler.WarningMailAsync(req, user, subject, text, useThisAddress);

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns it.
    /// </summary>
    public static Elements.Page CreatePage(Request req, string title)
    {
        Elements.Page page = Handler.CreatePage(req, title);
        page.Favicon = Handler.Favicon(req);
        page.Styles = Handler.Styles(req, out var fontUrl);
        if (fontUrl != null && Handler.PreloadFont)
            page.Preloads.Add(new Elements.Preload(fontUrl, "font"));
        Navigation(req, page);
        return page;
    }
    
    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns as an out parameter.
    /// </summary>
    public static void CreatePage(Request req, string title, out Elements.Page page)
    {
        page = CreatePage(req, title);
    }

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title, adds the favicon, navigation and style(s) from the preset, sets req.Page to the new page and returns it and its list of elements as an out parameter for easy access.
    /// </summary>
    public static void CreatePage(Request req, string title, out Elements.Page page, out List<Elements.IPageElement> e)
    {
        page = CreatePage(req, title);
        e = page.Elements;
    }

    /// <summary>
    /// Populates the navigation bar of the given page using information from the given request.
    /// </summary>
    public static void Navigation(Request req, Elements.Page page)
        => Handler.Navigation(req, page);

    /// <summary>
    /// Returns a list of styles that should be used for the given request as well as the URL of the used font in order to preload this if desired.
    /// </summary>
    public static List<IStyle> Styles(Request req, out string? fontUrl)
        => Handler.Styles(req, out fontUrl);

    /// <summary>
    /// Adds the script at /scripts[URL-MIDDLE].js to the given Page.
    /// </summary>
    public static void AddScript(this Elements.Page page, string urlMiddle)
        => page.Scripts.Add(new Elements.Script("/scripts" + urlMiddle + ".js"));

    /// <summary>
    /// Adds the default error script and the default error element (below the last current element or at the top if empty) to the page.
    /// </summary>
    public static void AddError(this Elements.Page page)
    {
        page.Scripts.Add(ErrorScript);
        page.Elements.Add(ErrorElement);
    }

    /// <summary>
    /// Sets the title of the page and adds a HeadingElement with the given subtext to it.
    /// </summary>
    public static void AddTitle(this Elements.Page page, string title, string text = "")
    {
        page.Title = title;
        page.Elements.Add(new Elements.HeadingElement(title, text));
    }

    /// <summary>
    /// Adds a button to contact customer support to the page.
    /// </summary>
    public static void AddSupportButton(this Elements.Page page, Request req)
        => Handler.AddSupportButton(req, page);

    /// <summary>
    /// The default error script.
    /// </summary>
    public static IScript ErrorScript
        => new Elements.CustomScript("let error = document.querySelector(\"#error\");\n\nfunction ShowError(message) {\n\terror.firstElementChild.innerText = message;\n\terror.style.display = \"block\";\n}\n\nfunction HideError() {\n\terror.style.display = \"none\";\n}");
    
    /// <summary>
    /// The default redirect script.
    /// </summary>
    public static IScript RedirectScript
        => new Elements.CustomScript("function Redirect() {\n\ttry {\n\t\tlet query = new URLSearchParams(window.location.search);\n\t\tif (query.has(\"redirect\"))\n\t\t{\n\t\t\tlet redirect = query.get(\"redirect\");\n\t\t\tif (redirect.startsWith(\"/\") || redirect.startsWith(\"https://\") || redirect.startsWith(\"http://\"))\n\t\t\t\twindow.location.assign(redirect);\n\t\t\telse window.location.assign(\"/\");\n\t\t}\n\t\telse window.location.assign(\"/\");\n\t} catch {\n\t\twindow.location.assign(\"/\");\n\t}\n}");

    /// <summary>
    /// The default redirect query script.
    /// </summary>
    public static IScript RedirectQueryScript
        => new Elements.CustomScript("function RedirectQuery() {\n\ttry {\n\t\tlet query = new URLSearchParams(window.location.search);\n\t\tif (query.has(\"redirect\"))\n\t\t{\n\t\t\tlet redirect = query.get(\"redirect\");\n\t\t\tif (redirect.startsWith(\"/\") || redirect.startsWith(\"https://\") || redirect.startsWith(\"http://\"))\n\t\t\t\treturn `?redirect=${encodeURIComponent(redirect)}`;\n\t\t\telse return \"\";\n\t\t}\n\t\telse return \"\";\n\t} catch {\n\t\treturn \"\"\n\t}\n}");

    /// <summary>
    /// The default script to send simple requests and get either the status code or the response text if the status code is 200, or null if an error occurred.
    /// </summary>
    public static IScript SendRequestScript
        => new Elements.CustomScript("async function SendRequest(url, method, statusOnly) {\n\tif (method === undefined)\n\t\tmethod = \"GET\";\n\tif (statusOnly === undefined)\n\t\tstatusOnly = false;\n\ttry {\n\t\tvar response = await fetch(url, {method:method});\n\t\tif (response.status === 200)\n\t\t\treturn statusOnly ? 200 : response.text();\n\t\telse return response.status;\n\t} catch (ex) {\n\t\tconsole.error(ex.message);\n\t\treturn null;\n\t}\n}");

    /// <summary>
    /// The default error element.
    /// </summary>
    public static Elements.ButtonElementJS ErrorElement
        => new(null, "Error", "HideError()", "red", "display: none;", "error");

    /// <summary>
    /// Returns an account navbar button (to log in or access the logged in account).
    /// </summary>
    public static Elements.IButton AccountButton(Request req)
        => Handler.AccountButton(req);

    /// <summary>
    /// Adds elements to allow for password (and 2FA if present) verification with input IDs 'password' and 'code'.
    /// </summary>
    public static void AddAuthElements(Elements.Page page, Request req)
        => Handler.AddAuthElements(page, req);

    /// <summary>
    /// Checks whether the given password (and 2FA code if necessary) provided in the query (keys 'password' and 'code') is correct for the given user.<br/>
    /// If it is correct, nothing else happens.
    /// If it is incorrect, "no" is forcefully written and the user is reported for failed authentication.
    /// </summary>
    public static async Task Auth(this Request req, User user)
    {
        string
            password = req.Query.GetOrThrow("password"),
            code = req.Query.GetOrThrow("code");
        
        if (!await req.UserTable.ValidatePasswordAsync(user.Id, password, null)
            || (user.TwoFactor.TOTPEnabled() && !await req.UserTable.ValidateTOTPAsync(user.Id, code, req, true)))
        {
            AccountManager.ReportFailedAuth(req);
            throw new ForcedResponse(new TextResponse("no"));
        }
    }

    /// <summary>
    /// The path of the login page.<br/>
    /// Default: [Handler.UsersPluginPath]/login
    /// </summary>
    public static string LoginPath(Request req)
        => Handler.LoginPath(req);
    
    /// <summary>
    /// Modifies the given default UI page after its initial construction.
    /// </summary>
    public static void ModifyPage(Request req, Page page)
        => Handler.ModifyPage(req, page);
}
