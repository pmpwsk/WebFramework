using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains additional data for an authentication token (expiration, 2FA state).
/// </summary>
[DataContract]
public class AuthTokenData
{
    /// <summary>
    /// The date and time of the token's expiration.
    /// </summary>
    [DataMember] public DateTime Expires { get; private set; }

    /// <summary>
    /// Whether the token still needs two-factor authentication to finish its login process.
    /// </summary>
    [DataMember] public bool Needs2FA { get; private set; }

    /// <summary>
    /// Creates a new object for additional data of an authentication token.
    /// </summary>
    /// <param name="needs2FA">Whether the token still needs two-factor authentication.</param>
    public AuthTokenData(bool needs2FA, bool temporary)
    {
        Expires = DateTime.UtcNow + (temporary ? TimeSpan.FromMinutes(10) : AccountManager.Settings.TokenExpiration);
        Needs2FA = needs2FA;
    }
}