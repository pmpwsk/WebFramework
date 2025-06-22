using DnsClient;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace uwap.WebFramework.Mail;

public static partial class MailManager
{
    /// <summary>
    /// Manages outbound emails.
    /// </summary>
    public static partial class Out
    {
        /// <summary>
        /// The method that is called after a mail message was sent (regardless of whether the attempt was successful or not).
        /// </summary>
        public static readonly SubscriberContainer<SentDelegate> MailSent = new();

        /// <summary>
        /// The method that is called to determine whether a mail message should be sent to the given recipient externally (returning true to send externally, false otherwise).
        /// </summary>
        public static readonly SubscriberContainer<BeforeSendDelegate> BeforeSend = new();

        /// <summary>
        /// Whether to attempt to send emails directly.<br/>
        /// Default: true
        /// </summary>
        public static bool EnableFromSelf { get; set; } = true;

        /// <summary>
        /// Whether to allow sending emails using IPv6.<br/>
        /// Default: true<br/>
        /// This is mostly used to make sure that the receiving server sees the IPv4 address (for SPF) in the case of a static IPv4 address and dynamic IPv6 address.
        /// </summary>
        public static bool AllowIPv6 { get; set; } = true;

        /// <summary>
        /// The object that should attempt to send emails that couldn't be sent directly or null if no alternative sending method should be used.<br/>
        /// Default: null
        /// </summary>
        public static IMailBackup? BackupSender { get; set; } = null;

        /// <summary>
        /// The selector of the DKIM key to use or null if messages shouldn't be signed.<br/>
        /// Default: default
        /// </summary>
        public static string? DkimSelector { get; set; } = "default";

        /// <summary>
        /// The maximum amount of time to wait before giving up on a mail sending attempt (in milliseconds).<br/>
        /// Default: 10000
        /// </summary>
        public static int Timeout { get; set; } = 10000;

        /// <summary>
        /// Generates a mail message using the given information, signs it (if possible) and sends it to the appropriate server.
        /// </summary>
        public static MailSendResult Send(MailboxAddress from, MailboxAddress to, string subject, string text, bool isHtml, bool allowBackup = true)
            => Send(from, [to], subject, text, isHtml, out _, allowBackup);

        /// <summary>
        /// Generates a mail message using the given information, signs it (if possible) and sends it to the appropriate server, and returns the resulting message IDs as a list.
        /// </summary>
        public static MailSendResult Send(MailboxAddress from, MailboxAddress to, string subject, string text, bool isHtml, out List<string> messageIds, bool allowBackup = true)
            => Send(from, [to], subject, text, isHtml, out messageIds, allowBackup);

        /// <summary>
        /// Generates a mail message for each of the recipients, signs them (if possible) and sends them to the appropriate servers.
        /// </summary>
        public static MailSendResult Send(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string text, bool isHtml, bool allowBackup = true)
            => Send(from, to, subject, text, isHtml, out _, allowBackup);

        /// <summary>
        /// Generates a mail message for each of the recipients, signs them (if possible) and sends them to the appropriate servers, and returns the resulting message IDs as a list.
        /// </summary>
        public static MailSendResult Send(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string text, bool isHtml, out List<string> messageIds, bool allowBackup = true)
            => Send(isHtml ? new MailGen(from, to, subject, null, text) : new MailGen(from, to, subject, text, null), out messageIds, allowBackup);

        /// <summary>
        /// Generates a mail message for each of the recipients, signs them (if possible) and sends them to the appropriate servers, and returns the resulting message IDs as a list.
        /// </summary>
        public static MailSendResult Send(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string? textBody, string? htmlBody, out List<string> messageIds, bool allowBackup = true)
            => Send(new MailGen(from, to, subject, textBody, htmlBody), out messageIds, allowBackup);

        /// <summary>
        /// Generates a mail message for each of the recipients, signs them (if possible) and sends them to the appropriate servers.
        /// </summary>
        public static MailSendResult Send(MailGen mailGen, bool allowBackup = true)
            => Send(mailGen, out _, allowBackup);

        /// <summary>
        /// Generates a mail message for each of the recipients, signs them (if possible) and sends them to the appropriate servers, and returns the resulting message IDs as a list.
        /// </summary>
        public static MailSendResult Send(MailGen mailGen, out List<string> messageIds, bool allowBackup = true)
        {
            messageIds = [];
            List<MailboxAddress> leftAddresses = [];
            Dictionary<MailboxAddress, string> internalLog = [];
            foreach (var a in mailGen.To)
            {
                string id = ServerDomain == null ? MimeUtils.GenerateMessageId() : MimeUtils.GenerateMessageId(ServerDomain);
                string? log = null;
                if (BeforeSend.InvokeAndGet(s => s(mailGen, a, id, out log), _ => {}).All(r => r))
                    leftAddresses.Add(a);
                else
                {
                    messageIds.Add(id);
                    internalLog[a] = log ?? "[No log]";
                }
            }
            MailSendResult.Attempt? fromSelf = null;
            if (EnableFromSelf && leftAddresses.Count != 0)
                fromSelf = SendFromSelf(mailGen, leftAddresses, messageIds);
            MailSendResult.Attempt? fromBackup = null;
            if (allowBackup && BackupSender != null && leftAddresses.Count != 0)
            {
                try
                {
                    fromBackup = BackupSender.Send(GenerateMessage(mailGen, true, out var messageId, leftAddresses));
                    messageIds.Add(messageId);
                }
                catch (Exception ex)
                {
                    fromBackup = new(MailSendResult.ResultType.Failed, [$"Error: {ex.Message}"]);
                }
            }

            MailSendResult result = new(internalLog, fromSelf, fromBackup);

            InvokeMailSent(GenerateMessage(mailGen, true, out var messageId2), result);
            messageIds.Add(messageId2);
            return result;
        }

        /// <summary>
        /// Generates a mail message using the given message object.
        /// </summary>
        private static MimeMessage GenerateMessage(MailGen mailGen, bool sign, out string messageId, IEnumerable<MailboxAddress>? replaceToWithThis = null)
        {
            if (replaceToWithThis == null)
            {
                if (!mailGen.To.Any())
                    throw new Exception("No recipient was set.");
            }
            else if (!replaceToWithThis.Any())
                throw new Exception("No recipient was set.");

            var message = new MimeMessage();

            message.From.Add(mailGen.From);
            foreach (var t in replaceToWithThis ?? mailGen.To)
                message.To.Add(t);
            message.Subject = mailGen.Subject;

            var builder = new BodyBuilder();
            if (mailGen.TextBody != null)
                builder.TextBody = mailGen.TextBody;
            if (mailGen.HtmlBody != null)
                builder.HtmlBody = mailGen.HtmlBody;
            foreach (var attachment in mailGen.Attachments)
            {
                if (attachment.ContentType != null)
                {
                    try
                    {
                        builder.Attachments.Add(attachment.Name, attachment.Bytes, ContentType.Parse(attachment.ContentType));
                        continue;
                    }
                    catch { }
                }
                builder.Attachments.Add(attachment.Name, attachment.Bytes);
            }
            message.Body = builder.ToMessageBody();

            messageId = ServerDomain == null ? MimeUtils.GenerateMessageId() : MimeUtils.GenerateMessageId(ServerDomain);
            message.MessageId = messageId;

            if (mailGen.IsReplyToMessageId != null)
                message.InReplyTo = mailGen.IsReplyToMessageId;

            mailGen.CustomChange?.Invoke(message);

            if (sign)
                Sign(message);

            return message;
        }

        /// <summary>
        /// Signs the given mail message or does nothing in case no DKIM selector was set.
        /// </summary>
        private static void Sign(MimeMessage message)
        {
            if (DkimSelector != null)
            {
                var from = message.From.Mailboxes.First();
                string dkimPath = $"../DKIM/{from.Domain}.pem";
                if (File.Exists(dkimPath))
                {
                    var signer = new DkimSigner(dkimPath, from.Domain, DkimSelector);
                    signer.Sign(message, new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.To });
                }
            }
        }

        /// <summary>
        /// Calls the method that should be called after a message was attempted to be sent.
        /// </summary>
        public static void InvokeMailSent(MimeMessage message, MailSendResult result)
            => MailSent.Invoke(s => s(message, result), _ => {});

        /// <summary>
        /// Turns the given IP (key) and associated server domain (value, if known) into a string to be used in connection logs.
        /// </summary>
        private static string ServerString(KeyValuePair<string, string?> domain)
            => domain.Value == null ? domain.Key : $"{domain.Value} ({domain.Key})";

        /// <summary>
        /// Attempts to send the given email directly.
        /// </summary>
        private static MailSendResult.Attempt SendFromSelf(MailGen mailGen, List<MailboxAddress> leftAddresses, List<string> messageIds)
        {
            List<string> log = [];
            try
            {
                MailSendResult.ResultType? resultType = null;
                HashSet<string> failedServers = [];
                Dictionary<MailboxAddress, Dictionary<string, string?>> serversForAddresses = [];
                foreach (string mailDomain in leftAddresses.Select(x => x.Domain).Distinct())
                {
                    var servers = GetServers(mailDomain);
                    if (servers.Count != 0)
                    {
                        foreach (var a in leftAddresses)
                            if (a.Domain == mailDomain)
                                serversForAddresses[a] = servers;
                    }
                    else log.Add($"No mail servers found for '{mailDomain}'.");
                }
                if (serversForAddresses.Count == 0)
                    throw new Exception("No mail servers were found.");

                using var client = new SmtpClient();
                string? connectedServer = null;
                if (ServerDomain != null)
                    client.LocalDomain = ServerDomain;
                while (serversForAddresses.Count != 0)
                {
                    KeyValuePair<MailboxAddress, Dictionary<string, string?>> due;
                    if (connectedServer != null && client.IsConnected)
                    {
                        var acceptable = serversForAddresses.Where(x => x.Value.ContainsKey(connectedServer));
                        if (acceptable.Any())
                            due = acceptable.First();
                        else
                        {
                            client.Disconnect(true);
                            log.Add("Disconnected.");
                            due = serversForAddresses.First();
                        }
                    }
                    else due = serversForAddresses.First();
                    log.Add($"{due.Key.Address}...");
                    bool success = false;
                    try
                    {
                        if (!client.IsConnected)
                        {
                            if (due.Value.Count == 0)
                            {
                                log.Add($"No mail servers left.");
                                continue;
                            }
                            foreach (var server in due.Value)
                            {
                                if (failedServers.Contains(server.Key))
                                    continue;
                                using var cts = new CancellationTokenSource(Timeout);
                                string suitableFor = Parsers.EnumerationText(serversForAddresses.Where(x => x.Value.ContainsKey(server.Key)).Select(x => x.Key.Address));
                                try
                                {
                                    Stopwatch stopwatch = Stopwatch.StartNew();
                                    Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
                                    socket.Connect(server.Key, 25);
                                    client.Connect(socket, server.Value ?? server.Key, 25, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, cts.Token);
                                    stopwatch.Stop();
                                    log.Add($"Connected {(client.IsSecure?"":"in")}securely to {ServerString(server)} (suitable for {suitableFor}) after {stopwatch.ElapsedMilliseconds}ms.");
                                    if (client.IsConnected)
                                    {
                                        connectedServer = server.Key;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedServers.Add(server.Key);
                                    log.Add($"Failed to connect to {ServerString(server)} (suitable for {suitableFor}): {ex.Message}");
                                }
                            }

                            if (!client.IsConnected)
                            {
                                log.Add($"Failed to connect to a mail server for {due.Key.Address}.");
                                continue;
                            }
                        }

                        try
                        {
                            string response = client.Send(GenerateMessage(mailGen, true, out string messageId, [due.Key]));
                            messageIds.Add(messageId);
                            success = true;
                            leftAddresses.Remove(due.Key);
                            log.Add($"Response: {response}");
                        }
                        catch (SmtpCommandException ex)
                        {
                            log.Add($"SMTP error: {ex.Message} {ex.StatusCode} {ex.ErrorCode} {ex.HelpLink}");
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Add($"Failed: {ex.Message}");
                    }
                    finally
                    {
                        if (resultType == null)
                            resultType = success ? MailSendResult.ResultType.Success : MailSendResult.ResultType.Failed;
                        else if ((success && resultType == MailSendResult.ResultType.Failed) || ((!success) && resultType == MailSendResult.ResultType.Success))
                            resultType = MailSendResult.ResultType.Mixed;

                        serversForAddresses.Remove(due.Key);
                    }
                }
                if (client.IsConnected)
                {
                    client.Disconnect(true);
                    log.Add("Disconnected.");
                }
                client.Dispose();
                return new MailSendResult.Attempt(resultType ?? MailSendResult.ResultType.Failed, log);
            }
            catch (Exception ex)
            {
                log.Add($"Error: {ex.Message}");
                return new MailSendResult.Attempt(MailSendResult.ResultType.Failed, log);
            }
        }

        /// <summary>
        /// Resolves the given domain to find its mail servers, fully resolves those and returns them as a dictionary (key = IP, value = server domain if known).
        /// </summary>
        private static Dictionary<string, string?> GetServers(string mailDomain)
        {
            try
            {
                Dictionary<string, string?> results = []; //key is ip, value is domain or null if raw ip

                foreach (string dns in SortedMxList(mailDomain))
                {
                    if (IPAddress.TryParse(dns, out var ip))
                    {
                        if ((AllowIPv6 || ip.AddressFamily == AddressFamily.InterNetwork) && !results.ContainsKey(dns)) //InterNetwork means IPv4
                            results[dns] = null;
                    }
                    else
                    {
                        if (AllowIPv6)
                        {
                            var v6 = DnsLookup.Query(dns, QueryType.AAAA).Answers.AaaaRecords().FirstOrDefault();
                            if (v6 != null)
                            {
                                string v6String = v6.Address.ToString();
                                if ((!results.TryGetValue(v6String, out var oldV6)) || oldV6 == null)
                                    results[v6String] = dns;
                            }
                        }

                        var v4 = DnsLookup.Query(dns, QueryType.A).Answers.ARecords().FirstOrDefault();
                        if (v4 != null)
                        {
                            string v4String = v4.Address.ToString();
                            if ((!results.TryGetValue(v4String, out var oldV4)) || oldV4 == null)
                                results[v4String] = dns;
                        }
                    }
                }
                return results;
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// The random number generator to randomly sort mail servers with equal priority.
        /// </summary>
        private static readonly Random RNG = new();

        /// <summary>
        /// Resolves the given domain to find its mail servers (could be IPs or domains) and returns them as a list, sorted by their priority (servers with equal priority are randomly sorted).
        /// </summary>
        private static List<string> SortedMxList(string domain)
        {
            var result = DnsLookup.Query(domain, QueryType.MX);
            var records = result.Answers.MxRecords();
            Dictionary<ushort, List<string>> dict = [];
            foreach (var mx in records)
            {
                if (!dict.TryGetValue(mx.Preference, out var l))
                {
                    l = [];
                    dict[mx.Preference] = l;
                }
                l.Add(mx.Exchange.Value.TrimEnd('.'));
            }
            List<string> list = [];
            foreach (var pair in dict.OrderBy(pair => pair.Key))
            {
                var l = pair.Value;
                while (l.Count > 0)
                {
                    int index = RNG.Next(l.Count);
                    list.Add(l[index]);
                    l.RemoveAt(index);
                }
            }
            return list.Distinct().ToList();
        }
    }
}
