namespace uwap.WebFramework.Accounts.Certificates;

/// <summary>
/// Supported algorithms for certificate identification.
/// </summary>
public enum PublicKeyAlgorithm
{
    RSA = 0,
    DSA = 1,
    ECDsa = 2,
    ECDiffieHellman = 3
}