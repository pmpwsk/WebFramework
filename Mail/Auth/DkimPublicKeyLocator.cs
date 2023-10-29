using MimeKit.Cryptography;
using Org.BouncyCastle.Crypto;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    private class DkimPublicKeyLocator : DkimPublicKeyLocatorBase
    {
        public override AsymmetricKeyParameter LocatePublicKey(string methods, string domain, string selector, CancellationToken cancellationToken = default)
        {
            if (!methods.Split(':').Contains("dns/txt"))
                throw new NotSupportedException("'methods' doesn't include \"dns/txt\".");

            var query = MailManager.DnsLookup.Query(selector + "._domainkey." + domain, DnsClient.QueryType.TXT);
            var records = query.Answers.TxtRecords();

            return GetPublicKey(string.Join("", records.Select(x => string.Join("", x.Text))));
        }

        public override Task<AsymmetricKeyParameter> LocatePublicKeyAsync(string methods, string domain, string selector, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => {
                return LocatePublicKey(methods, domain, selector, cancellationToken);
            }, cancellationToken);
        }
    }
}