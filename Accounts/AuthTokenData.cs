using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains additional data for an authentication token (expiration, 2FA state).
/// </summary>
/// <param name="needs2FA">Whether the token still needs two-factor authentication.</param>
[DataContract]
public class AuthTokenData(bool needs2FA, bool temporary, string? friendlyName, ReadOnlyCollection<string>? limitedToPaths)
{
    /// <summary>
    /// The date and time of the token's expiration.
    /// </summary>
    [DataMember] public DateTime Expires { get; private set; } = DateTime.UtcNow + (temporary ? TimeSpan.FromMinutes(10) : Server.Config.Accounts.TokenExpiration);

    /// <summary>
    /// Whether the token still needs two-factor authentication to finish its login process.
    /// </summary>
    [DataMember] public bool Needs2FA { get; private set; } = needs2FA;

    /// <summary>
    /// A name for this token, e.g. what device it is used on or what application requested it.
    /// </summary>
    [DataMember] public string? FriendlyName { get; private set; } = friendlyName;

    /// <summary>
    /// The list of paths the token is limited to or null if it's not limited.
    /// </summary>
    [DataMember] public ReadOnlyCollection<string>? LimitedToPaths { get; private set; } = limitedToPaths;
}