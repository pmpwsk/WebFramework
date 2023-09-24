using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data for a mail message so it can be generated and sent out later.
/// </summary>
public class MailGen
{
    /// <summary>
    /// The address (and name) the email was sent from.
    /// </summary>
    public MailboxAddress From;

    /// <summary>
    /// The collection of addresses (and names) the email should be sent to.
    /// </summary>
    public IEnumerable<MailboxAddress> To;

    /// <summary>
    /// The subject of the email.
    /// </summary>
    public string Subject;

    /// <summary>
    /// The text of the email.
    /// </summary>
    public string Text;

    /// <summary>
    /// Whether the text contains HTML code or not.
    /// </summary>
    public bool IsHtml;

    /// <summary>
    /// The ID of the message this message is a reply to, or null if it's not meant to be a reply.
    /// </summary>
    public string? IsReplyToMessageId;

    /// <summary>
    /// Creates a new object to generate a mail message.
    /// </summary>
    public MailGen(MailboxAddress from, MailboxAddress to, string subject, string text, bool isHtml)
        : this(from, new[] { to }, subject, text, isHtml) { }

    /// <summary>
    /// Creates a new object to generate a mail message.
    /// </summary>
    public MailGen(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string text, bool isHtml)
    {
        From = from;
        To = to;
        Subject = subject;
        Text = text;
        IsHtml = isHtml;
        IsReplyToMessageId = null;
    }
}