using MailKit.Net.Smtp;
using MimeKit;
using System.Net;

namespace uwap.WebFramework.Mail;

/// <summary>
/// A mail sending mechanism that attempts to send mail messages over an SMTP relay that can be used as a backup in case an email couldn't be sent directly.
/// </summary>
public class MailBackupRelay(string address, int port, NetworkCredential? credentials) : IMailBackup
{
    /// <summary>
    /// The address of the SMTP relay.
    /// </summary>
    public string Address = address;

    /// <summary>
    /// The port of the SMTP relay.
    /// </summary>
    public int Port = port;

    /// <summary>
    /// The credentials for the SMTP relay or null if no credentials should be used.
    /// </summary>
    public NetworkCredential? Credentials = credentials;

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
            if (Credentials != null)
                client.Authenticate(Credentials);
            string response = client.Send(message);
            client.Disconnect(true);
            return new(MailSendResult.ResultType.Success, [$"Response: {response}"]);
        }
        catch (Exception ex)
        {
            return new(MailSendResult.ResultType.Failed, [$"Error: {ex.Message}"]);
        }
    }
}
