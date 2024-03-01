using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains the two-factor authentication data for an account (the account saves null if 2FA is disabled).
/// </summary>
[DataContract]
public class TwoFactorData_Old
{
    /// <summary>
    /// Whether this two-factor authentication object has been verified.<br/>
    /// If it is not verified, it shouldn't be required to log in.
    /// </summary>
    [DataMember] public bool Verified { get; internal set; } = false;

    /// <summary>
    /// The private/secret key using which two-factor codes are generated (as a byte array).
    /// </summary>
    [DataMember] internal byte[] _SecretKey = [];

    /// <summary>
    /// The list of recovery codes that are still available to use.
    /// </summary>
    [DataMember] internal List<string> RecoveryCodes = [];
}