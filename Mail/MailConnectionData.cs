using SmtpServer;
using System.Net;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains the connection data of a received mail message.
/// </summary>
public class MailConnectionData(ISessionContext context)
{
    /// <summary>
    /// The IP endpoint the mail message was sent from.
    /// </summary>
    public IPEndPoint IP = (IPEndPoint)context.Properties["EndpointListener:RemoteEndPoint"];

    /// <summary>
    /// Whether the mail message was received securely (encrypted communication with the last mail transfer agent).
    /// </summary>
    public bool Secure = context.Pipe.IsSecure;
}
