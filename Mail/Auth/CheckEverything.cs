using MimeKit;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    public static FullResult CheckEverything(MailConnectionData connectionData, MimeMessage message, List<string>? logToPopulate)
        => new(connectionData, message, logToPopulate);
}