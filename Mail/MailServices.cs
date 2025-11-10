using SmtpServer.Mail;
using SmtpServer.Storage;
using SmtpServer;
using System.Buffers;
using SmtpServer.Protocol;
using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Mail;

public static partial class MailManager
{
    public static partial class In
    {
        /// <summary>
        /// Receives emails, parses them, evaluates their mail authentication and calls the set mail handling method.
        /// </summary>
        private class Store : MessageStore
        {
            /// <summary>
            /// Receives emails, parses them, evaluates their mail authentication and calls the set mail handling method.
            /// </summary>
            public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                try
                {
                    if (HandleMail.IsEmpty())
                        return SmtpResponse.MailboxUnavailable;

                    await using var stream = new MemoryStream();

                    var position = buffer.GetPosition(0);
                    while (buffer.TryGet(ref position, out var memory))
                        await stream.WriteAsync(memory, cancellationToken);

                    stream.Position = 0;

                    var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
                    var connectionData = new MailConnectionData(context);
                    var results = HandleMail.InvokeAndGet(s => s(context, message, connectionData), null);
                    return results.Count == 0 ? SmtpResponse.MailboxUnavailable : results.First();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error handling email: " + ex.Message);
                    return SmtpResponse.TransactionFailed;
                }
            }
        }

        /// <summary>
        /// Checks emails' sizes and calls the set mail accepting method to decide whether it should be accepted or not.
        /// </summary>
        private class Filter : MailboxFilter
        {
            /// <summary>
            /// Declines a message if it is too large.
            /// </summary>
            public override Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken ct)
            {
                if (from == Mailbox.Empty)
                    throw new SmtpResponseException(SmtpResponse.MailboxNameNotAllowed);
                else if (size > SizeLimit)
                    throw new SmtpResponseException(SmtpResponse.SizeLimitExceeded);

                return Task.FromResult(true);
            }

            /// <summary>
            /// Calls the MailboxExists method and forwards the result.
            /// </summary>
            public override Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken ct)
                => Task.FromResult(MailboxExists.InvokeAndGet(s => s(context, from, to), _ => {}).Any(r => r));
        }

        /// <summary>
        /// Returns the current certificate for the mail server.
        /// </summary>
        private class CertificateFactory : ICertificateFactory
        {
            /// <summary>
            /// Returns the current certificate for the mail server.
            /// </summary>
            public X509Certificate GetServerCertificate(ISessionContext sessionContext)
                => WebFramework.Server.GetCertificate(ServerDomain ?? throw new Exception("ServerDomain is null!")) ?? throw new Exception("No certificate was mapped to the server domain!");
        }
    }
}
