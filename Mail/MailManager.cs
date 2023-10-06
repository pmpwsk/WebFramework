using MimeKit.Cryptography;
using MimeKit.Utils;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using System.Net;
using SmtpServer.Protocol;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Mail;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Manages inbound and outbound emails.
/// </summary>
public static partial class MailManager
{
    /// <summary>
    /// The domain that should be used by this mail server in protocol welcome messages, outgoing mails and more.<br/>
    /// Default: null
    /// </summary>
    public static string? ServerDomain = null;

    /// <summary>
    /// The DNS servers that should be used to look up mail servers (to send mail and evaluate mail authentication).<br/>
    /// Default: none set, so the DNS server(s) in the network settings will be used
    /// </summary>
    public static IPEndPoint[]? DnsServers
    {
        set
        {
            if (value == null) DnsLookup = new();
            else DnsLookup = new(new LookupClientOptions(value));
        }
    }

    /// <summary>
    /// The DNS lookup object that should be used to look up mail servers (to send mail and evaluate mail authentication).
    /// </summary>
    public static LookupClient DnsLookup { get; private set; } = new();
}
