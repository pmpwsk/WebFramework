using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data about a complete mail sending attempt (both directly and using the backup).
/// </summary>
public class MailSendResult
{
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
    public MailSendResult(Attempt? fromSelf, Attempt? fromBackup)
    {
        FromSelf = fromSelf;
        FromBackup = fromBackup;
    }

    /// <summary>
    /// Contains data about an individual mail sending attempt (either directly or using a backup).
    /// </summary>
    public class Attempt
    {
        /// <summary>
        /// Whether the attempt was successful.
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// The connection log as a list of lines.
        /// </summary>
        public readonly List<string> ConnectionLog;

        /// <summary>
        /// Creates a new object for data about an individual mail sending attempt using the given information.
        /// </summary>
        public Attempt(bool success, List<string> connectionLog)
        {
            Success = success;
            ConnectionLog = connectionLog;
        }
    }
}