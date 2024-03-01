using MimeKit;
using SmtpServer.Mail;
using SmtpServer;
using SmtpServer.Protocol;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Mail;

/// <summary>
/// A delegate that is used for methods that are called after a mail message was sent (regardless of whether the attempt was successful or not).
/// </summary>
public delegate void SentDelegate(MimeMessage message, MailSendResult result);

/// <summary>
/// A delegate that is used for methods that decide whether a mail message with the given information should be accepted or not.
/// </summary>
public delegate bool MailboxExistsDelegate(ISessionContext context, IMailbox from, IMailbox to);

/// <summary>
/// A delegate that is used for methods that handle given mail messages (along with the given mail context and authentication result) after they have been accepted by an accepting method.
/// </summary>
public delegate SmtpResponse HandleDelegate(ISessionContext context, MimeMessage message, MailConnectionData authResult);

/// <summary>
/// A delegate that is used for methods that determine whether a mail message should be sent to the given recipient externally (returning true to send externally, false otherwise).
/// </summary>
public delegate bool BeforeSendDelegate(MailGen mailGen, MailboxAddress currentRecipient, string potentialMessageId, [MaybeNullWhen(true)] out string log);
