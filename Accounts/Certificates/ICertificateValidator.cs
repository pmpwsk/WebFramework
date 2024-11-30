using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Accounts.Certificates;

/// <summary>
/// Interface for certificate validation.
/// </summary>
public interface ICertificateValidator
{
    public bool Validate(X509Certificate2 certificate, string? hostname);
}