using MimeKit;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    [DataContract]
    public class FullResult
    {
        [DataMember]
        public readonly string IPAddress;

        [DataMember]
        public readonly bool Secure;

        [DataMember]
        public readonly MailAuthVerdictSPF SPF;

        [DataMember]
        public readonly MailAuthVerdictDKIM DKIM;

        [DataMember]
        public readonly MailAuthVerdictDMARC DMARC;

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