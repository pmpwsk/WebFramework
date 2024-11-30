using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Accounts.Certificates;

/// <summary>
/// Contains all the needed data to identify a certificate.
/// </summary>
public class CertificateValidatorOwnCA : ICertificateValidator
{   
    public bool Validate(X509Certificate2 certificate, string? hostname)
    {
        if (hostname != null && !certificate.MatchesHostname(hostname))
            return false;
        
        using var chain = new X509Chain();

        try
        {
            return chain.Build(certificate) || chain.ChainStatus.All(s => s.Status switch
            {
                X509ChainStatusFlags.NoError or
                X509ChainStatusFlags.RevocationStatusUnknown or
                X509ChainStatusFlags.OfflineRevocation or
                X509ChainStatusFlags.PartialChain => true,
                _ => false
            });
        }
        finally
        {
            foreach (var chainElement in chain.ChainElements)
                chainElement.Certificate.Dispose();
        }
    }
}