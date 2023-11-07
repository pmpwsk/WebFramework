using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data about a complete mail sending attempt (both directly and using the backup).
/// </summary>
public class MailSendResult
{
    /// <summary>
    /// The possible types of results.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// The message was sent to all recipents.
        /// </summary>
        Success,

        /// <summary>
        /// The message was sent to some recipients, but not all of them.
        /// </summary>
        Mixed,

        /// <summary>
        /// The message was sent to none of the recipients.
        /// </summary>
        Failed
    }

    public readonly Dictionary<MailboxAddress, string> Internal;

    /// <summary>
    /// Data about the attempt to send the message directly or null if no such attempt was made.
    /// </summary>
    public readonly Attempt? FromSelf;

    /// <summary>
    /// Data about the attempt to send the message using the backup or null if no such attempt was made.
    /// </summary>
    public readonly Attempt? FromBackup;

    /// <summary>
    /// Creates a new object for data about a complete mail sending attempt using the given individual attempt objects.
    /// </summary>
    public MailSendResult(Dictionary<MailboxAddress, string> internalLog, Attempt? fromSelf, Attempt? fromBackup)
    {
        Internal = internalLog;
        FromSelf = fromSelf;
        FromBackup = fromBackup;
    }

    /// <summary>
    /// Contains data about an individual mail sending attempt (either directly or using a backup).
    /// </summary>
    public class Attempt
    {
        /// <summary>
        /// Whether the attempt was successful, mixed or failed.
        /// </summary>
        public readonly ResultType ResultType;

        /// <summary>
        /// The connection log as a list of lines.
        /// </summary>
        public readonly List<string> ConnectionLog;

        /// <summary>
        /// Creates a new object for data about an individual mail sending attempt using the given information.
        /// </summary>
        public Attempt(ResultType resultType, List<string> connectionLog)
        {
            ResultType = resultType;
            ConnectionLog = connectionLog;
        }
    }
}