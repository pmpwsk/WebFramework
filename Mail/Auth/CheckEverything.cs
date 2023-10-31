using MimeKit;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Calls all of the checking methods and fills the given log list (if present) with details that aren't saved otherwise.<br/>
    /// The MailConnectionData object is provided in the event for incoming mail messages.
    /// </summary>
    public static FullResult CheckEverything(MailConnectionData connectionData, MimeMessage message, List<string>? logToPopulate)
        => new(connectionData, message, logToPopulate);
}