using MimeKit;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Class to contain a full mail authentication evaluation (SPF, DKIM and DMARC) for a message.<br/>
    /// Returned by CheckEverything(...).
    /// </summary>
    [DataContract]
    public class FullResult
    {
        /// <summary>
        /// The IP address the message was sent from.
        /// </summary>
        [DataMember]
        public readonly string IPAddress;

        /// <summary>
        /// Whether the message was sent over an encrypted connection.
        /// </summary>
        [DataMember]
        public readonly bool Secure;

        /// <summary>
        /// The result of SPF checking.
        /// </summary>
        [DataMember]
        public readonly MailAuthVerdictSPF SPF;

        /// <summary>
        /// The result of DKIM checking.
        /// </summary>
        [DataMember]
        public readonly MailAuthVerdictDKIM DKIM;

        /// <summary>
        /// The result of DMARC checking.
        /// </summary>
        [DataMember]
        public readonly MailAuthVerdictDMARC DMARC;

        /// <summary>
        /// Calls all of the checking methods and fills the given log list (if present) with details that aren't saved otherwise.
        /// </summary>
        internal FullResult(MailConnectionData connectionData, MimeMessage message, List<string>? logToPopulate)
        {
            IPAddress = connectionData.IP.Address.ToString();
            Secure = connectionData.Secure;

            string fromDomain = message.From.Mailboxes.First().Domain;
            string? returnHeader = message.Headers[HeaderId.ReturnPath];
            string returnDomain = (returnHeader != null && MailboxAddress.TryParse(returnHeader, out var address)) ? address.Domain : fromDomain;

            SPF = CheckSPF(returnDomain, connectionData.IP.Address, out var spfPassedDomain);

            DKIM = CheckDKIM(message, out var dkimResults);

            DMARC = CheckDMARC(returnDomain, fromDomain, SPF, DKIM, dkimResults);

            if (logToPopulate != null)
            {
                logToPopulate.Add("From: " + IPAddress);
                logToPopulate.Add("Secure: " + Secure.ToString());

                logToPopulate.Add($"SPF: {SPF}{(spfPassedDomain == null ? "" : $" with {spfPassedDomain}")}");

                logToPopulate.Add($"DKIM: {DKIM}");
                foreach (var ds in dkimResults)
                    logToPopulate.Add($"DKIM (domain={ds.Key.Domain}, selector={ds.Key.Selector}): {(ds.Value ? "Pass" : "Fail")}");

                logToPopulate.Add($"DMARC: {DMARC}");
            }
        }
    }
}