using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Accounts.Certificates;

/// <summary>
/// Contains all the needed data to identify a certificate.
/// </summary>
[DataContract]
public class CertificateIdentifier(PublicKeyAlgorithm algorithm, int keySize, string keyValue) : ICertificateValidator
{
    [DataMember]
    public PublicKeyAlgorithm Algorithm = algorithm;
    
    [DataMember]
    public int KeySize = keySize;
    
    [DataMember]
    public string KeyValue = keyValue.Replace(":", "");
    
    public bool Validate(X509Certificate2 certificate, string? hostname)
        => Validate(certificate, hostname, false, out _);

    public bool Validate(X509Certificate2 certificate, string? hostname, bool acceptExpiredTrusted, out DateTime minExpiration)
    {
        minExpiration = DateTime.MinValue;
        
        if (hostname != null && !certificate.MatchesHostname(hostname))
            return false;
        
        using var chain = new X509Chain();

        try
        {
            chain.Build(certificate);

            bool result = false;

            foreach (var chainElement in chain.ChainElements)
            {    
                var c = chainElement.Certificate;

                if (minExpiration == DateTime.MinValue || minExpiration > c.NotAfter)
                    minExpiration = c.NotAfter;

                if (!result && MatchesSingle(c))
                    result = true;
                
                foreach (var status in chainElement.ChainElementStatus)
                    switch (status.Status)
                    {
                        case X509ChainStatusFlags.NoError:
                        case X509ChainStatusFlags.RevocationStatusUnknown:
                        case X509ChainStatusFlags.OfflineRevocation:
                        case X509ChainStatusFlags.UntrustedRoot:
                        case X509ChainStatusFlags.PartialChain:
                            break;
                        case X509ChainStatusFlags.NotTimeValid:
                            if (result && acceptExpiredTrusted)
                                break;
                            else return false;
                        default:
                            return false;
                    }
            }

            return result;
        }
        finally
        {
            foreach (var chainElement in chain.ChainElements)
                chainElement.Certificate.Dispose();
        }
    }

    private bool MatchesSingle(X509Certificate2 certificate)
    {
        if (certificate.GetPublicKeyString() != KeyValue)
            return false;

        var rsa = certificate.GetRSAPublicKey();
        if (rsa != null)
            return Algorithm == PublicKeyAlgorithm.RSA && rsa.KeySize == KeySize;

        var dsa = certificate.GetDSAPublicKey();
        if (dsa != null)
            return Algorithm == PublicKeyAlgorithm.DSA && dsa.KeySize == KeySize;

        var ecdsa = certificate.GetECDsaPublicKey();
        if (ecdsa != null)
            return Algorithm == PublicKeyAlgorithm.ECDsa && ecdsa.KeySize == KeySize;

        var ecdh = certificate.GetECDiffieHellmanPublicKey();
        if (ecdh != null)
            return Algorithm == PublicKeyAlgorithm.ECDiffieHellman && ecdh.KeySize == KeySize;

        return false;
    }
}