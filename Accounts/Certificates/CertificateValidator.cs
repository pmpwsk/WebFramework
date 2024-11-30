using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Accounts.Certificates;

/// <summary>
/// Validates a given certificate using the system defaults.
/// </summary>
public class CertificateValidator : ICertificateValidator
{
    public bool Validate(X509Certificate2 certificate, string? hostname)
    {
        if (hostname != null && !certificate.MatchesHostname(hostname))
            return false;
        
        using var chain = new X509Chain();

        try
        {
            return chain.Build(certificate);
        }
        finally
        {
            foreach (var chainElement in chain.ChainElements)
                chainElement.Certificate.Dispose();
        }
    }
}