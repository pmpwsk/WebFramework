using OtpNet;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

[DataContract]
public class TwoFactorTOTP
{
    [DataMember] public bool Verified { get; internal set; }

    [DataMember] public byte[] SecretKey {get; internal set; }

    [DataMember] internal List<string> Recovery;

    /// <summary>
    /// The 3 most recently used time steps. This is used so a code isn't used twice within its active duration.
    /// </summary>
    [DataMember] public List<long> UsedTimeSteps { get; internal set; } = [];

    /// <summary>
    /// The private/secret key using which two-factor codes are generated.
    /// </summary>
    public string SecretKeyString => Base32Encoding.ToString(SecretKey);

    /// <summary>
    /// The list of recovery codes that are still available to use.
    /// </summary>
    public ReadOnlyCollection<string> RecoveryCodes => Recovery.AsReadOnly();

    /// <summary>
    /// Generates a QR code (and converts it to base64) to add the secret key to an authenticator app (works with Google Authenticator) and returns the image source value for HTML.
    /// </summary>
    /// <param name="domain">The domain to be listed in the authenticator app.</param>
    /// <param name="username">The username to be listed in the authenticator app.</param>
    /// <returns></returns>
    public string QRImageBase64Src(string domain, string username)
        => Parsers.QRImageBase64Src($"otpauth://totp/{domain}:{username}?secret={SecretKey}&issuer={domain}");

    public TwoFactorTOTP()
    {
        Verified = false;
        SecretKey = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha1);
        Recovery = GenerateRecoveryCodes();
    }

    internal TwoFactorTOTP(TwoFactorData_Old old)
    {
        Verified = old.Verified;
        SecretKey = old._SecretKey;
        Recovery = old.RecoveryCodes;
    }
    
    internal void RemoveRecoveryCode(string code)
        => Recovery.Remove(code);
    
    internal void AddUsedTimestamp(long timestamp)
    {
        UsedTimeSteps.Add(timestamp);
        if (UsedTimeSteps.Count > 3)
            UsedTimeSteps.Remove(UsedTimeSteps.Min());
    }

    /// <summary>
    /// Generates a new list of recovery codes and returns it.<br/>
    /// As this is a static method, it doesn't replace any recovery code list with the generated one yet.
    /// </summary>
    public static List<string> GenerateRecoveryCodes()
    {
        List<string> result = [];
        while (result.Count < 8)
            result.Add(Parsers.RandomString(10, code => !result.Contains(code)));
        return result;
    }
}