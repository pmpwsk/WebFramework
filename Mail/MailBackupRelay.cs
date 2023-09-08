using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace uwap.WebFramework.Mail;

/// <summary>
/// A mail sending mechanism that attempts to send mail messages over an SMTP relay that can be used as a backup in case an email couldn't be sent directly.
/// </summary>
public class MailBackupRelay : IMailBackup
{
    /// <summary>
    /// The address of the SMTP relay.
    /// </summary>
    public string Address;

    /// <summary>
    /// The port of the SMTP relay.
    /// </summary>
    public int Port;

    /// <summary>
    /// The credentials for the SMTP relay or null if no credentials should be used.
    /// </summary>
    public NetworkCredential? Credentials;

    /// <summary>
    /// Creates a new object to send mail to the SMTP relay with the given information.
    /// </summary>
    public MailBackupRelay(string address, int port, NetworkCredential? credentials)
    {
        Address = address;
        Port = port;
        Credentials = credentials;
    }

    //documentation is inherited from the interface
    public MailSendResult.Attempt Send(MimeMessage message)
    {
        try
        {
            var client = new SmtpClient();
            using var cts = new CancellationTokenSource(MailManager.Out.Timeout);
            if (Port == 465)
                client.Connect(Address, 465, true, cts.Token);
            else client.Connect(Address, Port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, cts.Token);
            if (Credentials != null) client.Authenticate(Credentials);
            string response = client.Send(message);
            client.Disconnect(true);
            return new(true, new(), $"Response: {response}");
        }
        catch (Exception ex)
        {
            return new(false, new(), $"Error: {ex.Message}");
        }
    }
}
