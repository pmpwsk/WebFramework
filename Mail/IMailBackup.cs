using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Interface to create mail sending mechanisms that are used as a backup in case an email couldn't be sent directly.
/// </summary>
public interface IMailBackup
{
    /// <summary>
    /// Attempts to send the provided mail message and returns data about the attempt.
    /// </summary>
    public MailSendResult.Attempt Send(MimeMessage message);
}
