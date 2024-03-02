using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data for a mail message so it can be generated and sent out later.
/// </summary>
public class MailGen(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string? textBody, string? htmlBody)
{
    /// <summary>
    /// The address (and name) the email was sent from.
    /// </summary>
    public MailboxAddress From = from;

    /// <summary>
    /// The collection of addresses (and names) the email should be sent to.
    /// </summary>
    public IEnumerable<MailboxAddress> To = to;

    /// <summary>
    /// The subject of the email.
    /// </summary>
    public string Subject = subject;

    /// <summary>
    /// The text body of the email.
    /// </summary>
    public string? TextBody = textBody;

    /// <summary>
    /// The HTML body of the email.
    /// </summary>
    public string? HtmlBody = htmlBody;

    /// <summary>
    /// The ID of the message this message is a reply to, or null if it's not meant to be a reply.
    /// </summary>
    public string? IsReplyToMessageId = null;

    /// <summary>
    /// The list of attachments.
    /// </summary>
    public List<MailGenAttachment> Attachments = [];

    /// <summary>
    /// The function that should be applied after generating the message (but before signing), or null.
    /// </summary>
    public Action<MimeMessage>? CustomChange = null;

    /// <summary>
    /// Creates a new object to generate a mail message.
    /// </summary>
    public MailGen(MailboxAddress from, MailboxAddress to, string subject, string? textBody, string? htmlBody)
        : this(from, [to], subject, textBody, htmlBody) { }
}