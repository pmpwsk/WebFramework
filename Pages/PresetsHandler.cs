using MimeKit;
using System.Web;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Mail;

namespace uwap.WebFramework.Elements;

/// <summary>
/// Default preset handler class that should be inherited by a class overwriting some/all methods. Make sure to set Presets.Handler to an element of that new class once you have one.
/// </summary>
public class PresetsHandler
{
    /// <summary>
    /// The email address for support or null to use support@[MailManager.ServerDomain].<br/>
    /// Default implementation returns null
    /// </summary>
    protected virtual string? SupportEmail => null;

    /// <summary>
    /// Gets the the support email from SupportEmail but resolves the case of it being null.<br/>
    /// If SupportEmail and MailManager.ServerDomain are both null, null is returned here, too.<br/>
    /// If only SupportEmail is null and MailManager.ServerDomain isn't null, support@[MailManager.ServerDomain] is returned.<br/>
    /// </summary>
    protected virtual string? GetSupportEmail(Request req)
        => (SupportEmail == null && MailManager.ServerDomain == null) ? null : (SupportEmail ?? ("support@" + MailManager.ServerDomain));

    /// <summary>
    /// Sends an important email to the given user using the given subject and text. A different address may be specified.
    /// </summary>
    public virtual MailSendResult WarningMail(Request req, User user, string subject, string text, string? useThisAddress = null)
    {
        string? address = GetSupportEmail(req);
        if (address == null)
            return new([], null, null);
        return MailManager.Out.Send(
            new MailboxAddress(address, address),
            new MailboxAddress(user.Username, useThisAddress ?? user.MailAddress),
            subject,
            $"Hi, {user.Username}!\n{text}".Replace("\n", "<br />"),
            true, true);
    }

    /// <summary>
    /// Whether to add the primary font of Styles as a preload in CreatePage.<br/>
    /// Default: false
    /// </summary>
    public virtual bool PreloadFont
        => false;

    /// <summary>
    /// The favicon path to use in CreatePage for the given request.<br/>
    /// Default: null (uses favicon.ico if present)
    /// </summary>
    public virtual string? Favicon(Request req)
        => null;

    /// <summary>
    /// Creates a new Page (not IPage!) for the request with the given title and returns the new Page.
    /// </summary>
    public virtual Page CreatePage(Request req, string title)
        => new(title);

    /// <summary>
    /// Returns an account navbar button (to log in or access the logged in account).
    /// </summary>
    public virtual IButton AccountButton(Request req)
    => req.LoginState switch
        {
            LoginState.None
                => new Button("Login", ((IEnumerable<string>)["/", "/account/login", "/account/register"]).Contains(req.Path) || req.Path.StartsWith("/account/recovery")
                    ? ("/account/login" + req.CurrentRedirectQuery)
                    : "/account/login?redirect=" + HttpUtility.UrlEncode(req.Path + req.Context.Request.QueryString), "right"),
            LoginState.LoggedIn
                => new Button("Account", "/account/", "right"),
            LoginState.Banned
                => new Button("Banned", "#", "right"),
            _ => new Button("Logout", "/account/logout" + req.CurrentRedirectQuery, "right"),
        };

    /// <summary>
    /// Populates the navigation bar of the given page using information from the given request.
    /// </summary>
    public virtual void Navigation(Request req, Page page)
    {
        page.Navigation = [ new Button(req.Domain, "/") ];
        if (Server.Config.Accounts.Enabled)
            page.Navigation.Add(Presets.AccountButton(req));
    }

    /// <summary>
    /// Returns a list of styles that should be used for the given request as well as the URL of the used font in order to preload this if desired.<br/>
    /// Default implementation throws an exception because there are no default themes.
    /// </summary>
    public virtual List<IStyle> Styles(Request req, out string fontUrl)
        => throw new Exception("No style were set.");

    /// <summary>
    /// Adds a button to contact customer support to the page.
    /// </summary>
    public virtual void AddSupportButton(Request req, Page page)
    {
        string address = GetSupportEmail(req) ?? throw new Exception("No support email was found.");
        page.Elements.Add(new ButtonElement("Contact support", address, "mailto:" + address, newTab: true));
    }

    /// <summary>
    /// Adds elements to allow for password (and 2FA if present) verification with input IDs 'password' and 'code'.
    /// </summary>
    public virtual void AddAuthElements(Request req)
    {
        req.Init(out Page _, out var e);
        if (!req.LoggedIn) throw new Exception("Not logged in.");
        User user = req.User;
        string? twoFactorText = user.TwoFactor.TOTPEnabled() ? null : "disabled";
        string? twoFactorStyle = user.TwoFactor.TOTPEnabled() ? null : "display: none";
        e.Add(new ContainerElement(null, new List<IContent>
        {
            new Heading("Password:"),
            new TextBox("Enter your password...", null, "password", TextBoxRole.Password, "Continue()"),
            new Heading("2FA code / recovery:", styles: twoFactorStyle),
            new TextBox("Enter the current code...", twoFactorText, "code", TextBoxRole.NoSpellcheck, "Continue()", styles: twoFactorStyle)
        }));
    }
}