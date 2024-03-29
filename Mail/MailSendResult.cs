﻿using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data about a complete mail sending attempt (both directly and using the backup).
/// </summary>
public class MailSendResult(Dictionary<MailboxAddress, string> internalLog, MailSendResult.Attempt? fromSelf, MailSendResult.Attempt? fromBackup)
{
    /// <summary>
    /// The possible types of results.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// The message was sent to all recipients.
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

    public readonly Dictionary<MailboxAddress, string> Internal = internalLog;

    /// <summary>
    /// Data about the attempt to send the message directly or null if no such attempt was made.
    /// </summary>
    public readonly Attempt? FromSelf = fromSelf;

    /// <summary>
    /// Data about the attempt to send the message using the backup or null if no such attempt was made.
    /// </summary>
    public readonly Attempt? FromBackup = fromBackup;

    /// <summary>
    /// Contains data about an individual mail sending attempt (either directly or using a backup).
    /// </summary>
    /// <remarks>
    /// Creates a new object for data about an individual mail sending attempt using the given information.
    /// </remarks>
    public class Attempt(MailSendResult.ResultType resultType, List<string> connectionLog)
    {
        /// <summary>
        /// Whether the attempt was successful, mixed or failed.
        /// </summary>
        public readonly ResultType ResultType = resultType;

        /// <summary>
        /// The connection log as a list of lines.
        /// </summary>
        public readonly List<string> ConnectionLog = connectionLog;
    }
}