﻿using MimeKit;
using SmtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains the mail authentication evaluation data of a received mail message as well as other interesting data about how the message was sent.
/// </summary>
public class MailAuthResult
{
    /// <summary>
    /// The IP endpoint the mail message was sent from.
    /// </summary>
    public IPEndPoint IP;

    /// <summary>
    /// Whether the mail message was received securely (encrypted communication with the last mail transfer agent).
    /// </summary>
    public bool Secure;

    /// <summary>
    /// Evaluates the mail authentication of the given mail context and message, and creates a new object that contains them.
    /// </summary>
    public MailAuthResult(ISessionContext context, MimeMessage message)
    {
        IP = (IPEndPoint)context.Properties["EndpointListener:RemoteEndPoint"];
        Secure = context.Pipe.IsSecure;
    }
}
