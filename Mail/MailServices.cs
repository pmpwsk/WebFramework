using SmtpServer.Mail;
using SmtpServer.Storage;
using SmtpServer;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpServer.Protocol;

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
                    if (HandleMail == null) return SmtpResponse.MailboxUnavailable;

                    await using var stream = new MemoryStream();

                    var position = buffer.GetPosition(0);
                    while (buffer.TryGet(ref position, out var memory))
                    {
                        await stream.WriteAsync(memory, cancellationToken);
                    }

                    stream.Position = 0;

                    var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
                    return HandleMail.Invoke(context, message, new MailConnectionData(context, message));
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
            #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            /// <summary>
            /// Declines a message if it is too large.
            /// </summary>
            public override async Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken ct)
            {
                try
                {
                    if (from == Mailbox.Empty)
                    {
                        return MailboxFilterResult.NoPermanently;
                    }
                    else if (size > SizeLimit)
                    {
                        return MailboxFilterResult.SizeLimitExceeded;
                    }
                    return MailboxFilterResult.Yes;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error accepting email: " + ex.Message);
                    return MailboxFilterResult.NoTemporarily;
                }
            }

            /// <summary>
            /// Applies the fate decided by the accepting method.
            /// </summary>
            public override async Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken ct)
            {
                try
                {
                    if (AcceptMail != null)
                    {
                        return AcceptMail.Invoke(context, from, to);
                    }
                    else return MailboxFilterResult.NoTemporarily;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error accepting email using event: " + ex.Message);
                    return MailboxFilterResult.NoTemporarily;
                }
            }
            #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }
    }
}
