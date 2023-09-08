using System.Web;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Mail;

namespace uwap.WebFramework.Elements;

public static class Presets
{
    public static PresetsHandler Handler = new PresetsHandler();

    public static MailSendResult WarningMail(User user, string subject, string text, string? useThisAddress = null)
    {
        return Handler.WarningMail(user, subject, text, useThisAddress);
    }

    public static void CreatePage(AppRequest request, string title, out Page page, out List<IPageElement> e)
    {
        Handler.CreatePage(request, title, out page, out e);
    }

    public static void Navigation(AppRequest request, Page page)
    {
        Handler.Navigation(request, page);
    }

    public static string[] Themes => Handler.Themes;
    public static List<IStyle> Style(IRequest request, out string fontUrl)
        => Handler.Style(request, out fontUrl);
    public static string ThemeName(IRequest request)
        => Handler.ThemeName(request);

    public static void Init(this AppRequest request, out Page page, out List<IPageElement> elements)
    {
        if (request.Page == null)
        {
            throw new Exception("No page was set.");
        }
        page = (Page)request.Page;
        elements = page.Elements;
    }

    public static void AddScript(this AppRequest request)
    {
        if (request.Page != null)
            ((Page)request.Page).AddScript(request.Path);
    }

    public static void AddScript(this AppRequest request, string suffix)
    {
        if (request.Page != null)
            ((Page)request.Page).AddScript(request.Path + "-" + suffix);
    }

    public static void AddScript(this Page page, string linkMiddle)
    {
        page.Scripts.Add(new Script("/scripts" + linkMiddle + ".js"));
    }

    public static void AddError(this Page page)
    {
        page.Scripts.Add(ErrorScript);
        page.Elements.Add(ErrorElement);
    }

    public static void AddTitle(this Page page, string title, string text = "")
    {
        page.Title = title;
        page.Elements.Add(new HeadingElement(title, text));
    }

    public static void AddSupportLink(this Page page)
    {
        Handler.AddSupportLink(page);
    }

    public static Script ErrorScript => new Script("/scripts/error.js");
    public static Script RedirectScript => new Script("/scripts/redirect.js");

    public static ButtonElementJS ErrorElement => new ButtonElementJS(null, "Error", "HideError()", "red", "display: none;", "error");

    public static Button AccountButton(AppRequest request)
    {
        switch (request.LoginState)
        {
            case LoginState.None:
                if (new string[] { "/", "/account/login", "/account/register" }.Contains(request.Path) || request.Path.StartsWith("/account/recovery"))
                    return new Button("Login", "/account/login" + request.CurrentRedirectQuery(), "right");
                else return new Button("Login", "/account/login?redirect=" + HttpUtility.UrlEncode(request.Path + request.Context.Request.QueryString), "right");
            case LoginState.LoggedIn:
                return new Button(Handler.AccountText, "/account", "right");
            default:
                return new Button("Logout", "/account/logout" + request.CurrentRedirectQuery(), "right");
        }
    }

    public static void AddAuthElements(this AppRequest request)
    {
        Handler.AddAuthElements(request);
    }

    public static async Task<bool> Auth(this ApiRequest request, User user)
    {
        if (request.Query.ContainsKey("password") && request.Query.ContainsKey("code"))
        {
            string password = request.Query["password"], code = request.Query["code"];
            if (user.ValidatePassword(password, null))
            {
                if (user.TwoFactorEnabled && !user.Validate2FA(code, request))
                {
                    AccountManager.ReportFailedAuth(request.Context);
                    await request.Write("no");
                    return false;
                }
                else return true;
            }
            else
            {
                AccountManager.ReportFailedAuth(request.Context);
                await request.Write("no");
                return false;
            }
        }
        else
        {
            request.Status = 400;
            return false;
        }
    }
}
