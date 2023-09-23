﻿using DnsClient;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.Xml;

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
        public static event SentDelegate? MailSent;

        /// <summary>
        /// Whether to attempt to send emails directly.<br/>
        /// Default: true
        /// </summary>
        public static bool EnableFromSelf = true;

        /// <summary>
        /// Whether to allow sending emails using IPv6.<br/>
        /// Default: true<br/>
        /// This is mostly used to make sure that the receiving server sees the IPv4 address (for SPF) in the case of a static IPv4 address and dynamic IPv6 address.
        /// </summary>
        public static bool AllowIPv6 = true;

        /// <summary>
        /// The object that should attempt to send emails that couldn't be sent directly or null if no alternative sending method should be used.<br/>
        /// Default: null
        /// </summary>
        public static IMailBackup? BackupSender = null;

        /// <summary>
        /// The selector of the DKIM key to use or null if messages shouldn't be signed.<br/>
        /// Default: default
        /// </summary>
        public static string? DkimSelector = "default";

        /// <summary>
        /// The maximum amount of time to wait before giving up on a mail sending attempt (in milliseconds).<br/>
        /// Default: 5000
        /// </summary>
        public static int Timeout = 5000;

        /// <summary>
        /// Generates a mail message using the given information.
        /// </summary>
        public static MimeMessage GenerateMessage(MailboxAddress from, MailboxAddress to, string subject, string text, bool isHtml)
        {
            var message = new MimeMessage();

            message.From.Add(from);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = new TextPart(isHtml ? "html" : "plain")
            {
                Text = text,
            };

            if (ServerDomain != null)
                message.MessageId = MimeUtils.GenerateMessageId(ServerDomain);

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
        /// Attempts to send the given email - first directly (if enabled), then using the backup (if set and not forbidden).
        /// </summary>
        /// <param name="allowBackup">Whether to allow sending using a backup (if one has been set).</param>
        public static MailSendResult Send(MimeMessage message, bool allowBackup = true)
        {
            Sign(message);
            bool serversFound = true;
            MailSendResult.Attempt? fromSelf = null;
            if (EnableFromSelf)
            {
                if (message.To.Mailboxes.Count() > 1 || message.Cc.Mailboxes.Any() || message.Bcc.Any()) throw new Exception("Only one recipient is supported.");
                if (!message.To.Mailboxes.Any()) throw new Exception("No recipient was set.");
                MailboxAddress to = message.To.Mailboxes.First();
                var servers = GetServers(to.Domain);
                if (!servers.Any())
                {
                    serversFound = false;
                    fromSelf = new(MailSendResult.ResultType.Failed, new() { $"No mail server found for domain '{to.Domain}'." });
                }
                else
                {
                    fromSelf = SendFromSelf(message, servers);
                }
            }
            MailSendResult.Attempt? fromBackup = null;
            if ((fromSelf == null || fromSelf.ResultType != MailSendResult.ResultType.Success) && allowBackup && serversFound && BackupSender != null)
            {
                try
                {
                    fromBackup = BackupSender.Send(message);
                }
                catch (Exception ex)
                {
                    fromBackup = new(MailSendResult.ResultType.Failed, new() { $"Error: {ex.Message}" });
                }
            }

            MailSendResult result = new(fromSelf, fromBackup);

            InvokeMailSent(message, result);
            return result;
        }

        /// <summary>
        /// Calls the method that should be called after a message was attempted to be sent.
        /// </summary>
        public static void InvokeMailSent(MimeMessage message, MailSendResult result)
        {
            MailSent?.Invoke(message, result);
        }

        /// <summary>
        /// Turns the given IP (key) and associated server domain (value, if known) into a string to be used in connection logs.
        /// </summary>
        private static string ServerString(KeyValuePair<string, string?> domain)
        {
            if (domain.Value == null) return domain.Key;
            else return $"{domain.Value} ({domain.Key})";
        }

        /// <summary>
        /// Attempts to send the given email directly.
        /// </summary>
        private static MailSendResult.Attempt SendFromSelf(MimeMessage message, Dictionary<string, string?> servers)
        {
            List<string> connectionLog = new();
            try
            {
                using var client = new SmtpClient();
                if (ServerDomain != null)
                    client.LocalDomain = ServerDomain;
                foreach (var domain in servers)
                {
                    using var cts = new CancellationTokenSource(Timeout);
                    try
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(domain.Key, 25);
                        client.Connect(socket, domain.Value ?? domain.Key, 25, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, cts.Token);
                        stopwatch.Stop();
                        connectionLog.Add($"Connected to {ServerString(domain)} (Secure={client.IsSecure}) after {stopwatch.ElapsedMilliseconds}ms.");
                        if (client.IsConnected) break;
                    }
                    catch (Exception ex)
                    {
                        connectionLog.Add($"Error connecting to {ServerString(domain)}: {ex.Message}");
                    }
                }
                if (!client.IsConnected)
                {
                    connectionLog.Add("Failed to connect to a mail server.");
                    return new MailSendResult.Attempt(MailSendResult.ResultType.Failed, connectionLog);
                }

                try
                {
                    string response = client.Send(message);
                    connectionLog.Add($"Response: {response}");
                    return new MailSendResult.Attempt(MailSendResult.ResultType.Success, connectionLog);
                }
                catch (SmtpCommandException ex1)
                {
                    connectionLog.Add($"SMTP Error: {ex1.Message} {ex1.StatusCode} {ex1.ErrorCode} {ex1.HelpLink}");
                    return new MailSendResult.Attempt(MailSendResult.ResultType.Failed, connectionLog);
                }
                catch (Exception ex1)
                {
                    connectionLog.Add($"Error: {ex1.Message}");
                    return new MailSendResult.Attempt(MailSendResult.ResultType.Failed, connectionLog);
                }
                finally
                {
                    try
                    {
                        client.Disconnect(true);
                    }
                    catch { }
                }
            }
            catch (Exception ex2)
            {
                connectionLog.Add($"Error: {ex2.Message}");
                return new MailSendResult.Attempt(MailSendResult.ResultType.Failed, connectionLog);
            }
        }

        /// <summary>
        /// Resolves the given domain to find its mail servers, fully resolves those and returns them as a dictionary (key = IP, value = server domain if known).
        /// </summary>
        private static Dictionary<string, string?> GetServers(string mailDomain)
        {
            try
            {
                Dictionary<string, string?> results = new(); //key is ip, value is domain or null if raw ip

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
                return new();
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
            Dictionary<ushort, List<string>> dict = new();
            foreach (var mx in records)
            {
                if (!dict.ContainsKey(mx.Preference)) dict[mx.Preference] = new();
                dict[mx.Preference].Add(mx.Exchange.Value.TrimEnd('.'));
            }
            List<string> list = new();
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
