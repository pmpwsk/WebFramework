using MimeKit;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Mail;

namespace uwap.WebFramework.Elements;

public class PresetsHandler
{
    public virtual string? SupportEmail => null;
    internal string? GetSupportEmail()
    {
        if (SupportEmail == null && MailManager.ServerDomain == null)
            return null;
        return SupportEmail ?? ("support@" + MailManager.ServerDomain);
    }

    public virtual MailSendResult WarningMail(User user, string subject, string text, string? useThisAddress = null)
    {
        string? address = GetSupportEmail();
        if (address == null)
            return new(null, null);
        var message = MailManager.Out.GenerateMessage(
        new MailboxAddress(address, address),
        new MailboxAddress(user.Username, useThisAddress ?? user.MailAddress),
        subject,
        $"Hi, {user.Username}!\n{text}".Replace("\n", "<br />"),
        true);
        if (Server.DebugMode)
        {
            MailSendResult result = new(new(true, new() { "Debug mail server." }, "Not actually sent."), null);
            MailManager.Out.InvokeMailSent(message, result);
            return result;
        }
        return MailManager.Out.Send(message);
    }

    public virtual void CreatePage(AppRequest request, string title, out Page page, out List<IPageElement> e)
    {
        page = new(title);
        request.Page = page;
        e = page.Elements;
    }

    public virtual string AccountText => "Account";

    public virtual void Navigation(AppRequest request, Page page)
    {
        if (AccountManager.Settings.Enabled)
            page.Navigation = new List<IButton>
            {
                new Button(request.Domain, "/")
            };
        else if (request.LoggedIn)
            page.Navigation = new List<IButton>
            {
                new Button(request.Domain, "/"),
                Presets.AccountButton(request)
            };
        else
            page.Navigation = new List<IButton>
            {
                new Button(request.Domain, "/"),
                Presets.AccountButton(request)
            };
    }

    public virtual string[] Themes => throw new Exception("No themes were set.");
    public virtual List<IStyle> Style(IRequest request, out string fontUrl)
    {
        throw new Exception("No themes were set.");
    }
    public virtual string ThemeName(IRequest request)
    {
        throw new Exception("No themes were set.");
    }

    public virtual void AddSupportLink(Page page)
    {
        string? address = GetSupportEmail();
        if (address == null)
            throw new Exception("No support email was found.");
        page.Elements.Add(new ButtonElement("Contact support", address, "mailto:" + address, newTab: true));
    }

    public virtual void AddAuthElements(AppRequest request)
    {
        request.Init(out Page page, out var e);
        User? user = request.User;
        if (user == null) throw new Exception("Not logged in.");
        string? twoFactorText = user.TwoFactorEnabled ? null : "disabled";
        string? twoFactorStyle = user.TwoFactorEnabled ? null : "display: none";
        e.Add(new ContainerElement(null, new List<IContent>
        {
            new Heading("Password:"),
            new TextBox("Enter your password...", null, "password", TextBoxRole.Password, "Continue()"),
            new Heading("2FA code / recovery:", styles: twoFactorStyle),
            new TextBox("Enter the current code...", twoFactorText, "code", TextBoxRole.NoSpellcheck, "Continue()", styles: twoFactorStyle)
        }));
    }
}