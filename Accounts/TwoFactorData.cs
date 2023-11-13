using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using OtpNet;

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
    [DataMember] internal byte[] _SecretKey;
    /// <summary>
    /// The private/secret key using which two-factor codes are generated (as a base32 string).
    /// </summary>
    public string SecretKey => Base32Encoding.ToString(_SecretKey);

    /// <summary>
    /// The list of recovery codes that are still available to use.
    /// </summary>
    [DataMember] internal List<string> RecoveryCodes;
    /// <summary>
    /// The list of recovery codes that are still available to use (as a read-only list).
    /// </summary>
    public ReadOnlyCollection<string> Recovery => RecoveryCodes.AsReadOnly();

    /// <summary>
    /// The 3 most recently used time steps. This is used so a code isn't used twice within its active duration.<br/>
    /// This isn't persistent, so codes could be used twice when the server/application is restarted!
    /// </summary>
    private List<long> UsedTimeSteps = new();

    /// <summary>
    /// Creates a new object to save two-factor authentication data for an account using a new random secret key.
    /// </summary>
    public TwoFactorData_Old()
    {
        _SecretKey = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha1);
        RecoveryCodes = GenerateRecoveryCodes();
    }

    /// <summary>
    /// Creates a new object to save two-factor authentication data for an account using the given secret key.
    /// </summary>
    private TwoFactorData_Old(byte[] secretKey)
    {
        _SecretKey = secretKey;
        RecoveryCodes = GenerateRecoveryCodes();
    }

    /// <summary>
    /// Clones the current object and replaces the recovery codes of the new object with a new list of codes.
    /// </summary>
    public TwoFactorData_Old CloneWithNewRecovery()
        => new(_SecretKey);

    /// <summary>
    /// Generates a new list of recovery codes and returns it.<br/>
    /// As this is a static method, it doesn't replace any recovery code list with the generated one yet.
    /// </summary>
    private static List<string> GenerateRecoveryCodes()
    {
        List<string> result = new();
        while (result.Count < 8)
        {
            string code;
            do code = Parsers.RandomString(10);
            while (result.Contains(code));
            result.Add(code);
        }
        return result;
    }

    /// <summary>
    /// Checks whether the given code is valid right now and hasn't been used before.
    /// </summary>
    /// <param name="request">The current request (used to handle failed attempts and tokens).</param>
    /// <param name="updateDatabase">Whether to update the database entry afterwards (e.g. because the recovery code list changed).</param>
    /// <returns></returns>
    internal bool Validate(string code, IRequest request, out bool updateDatabase)
    {
        //banned?
        if (AccountManager.IsBanned(request.Context))
        {
            updateDatabase = false;
            return false;
        }

        //recovery code?
        if (RecoveryCodes.Contains(code))
        {
            RecoveryCodes.Remove(code);
            updateDatabase = true;
            return true;
        }

        UsedTimeSteps ??= new List<long>();

        //valid and unused?
        if (new Totp(_SecretKey).VerifyTotp(code, out long timeStepMatched, new VerificationWindow(1, 1)) && !UsedTimeSteps.Contains(timeStepMatched))
        {
            //add to used time stepts (and remove the oldest one if the list is full)
            UsedTimeSteps.Add(timeStepMatched);
            if (UsedTimeSteps.Count > 3) UsedTimeSteps.Remove(UsedTimeSteps.Min());

            //does an auth token even exist? (this should be the case, but you never know)
            if (request.Cookies.Contains("AuthToken"))
            {
                //get and decode the auth token to find the user (assuming that it's the user of the current 2FA object)
                string combinedToken = request.Cookies["AuthToken"];
                string id = combinedToken.Remove(12);
                string authToken = combinedToken.Remove(0, 12);
                var users = request.UserTable;
                if (users != null && users.TryGetValue(id, out var user))
                {
                    //set Needs2FA to false for the current auth token if it exists
                    if (user.Auth.Exists(authToken))
                    {
                        user.Auth[authToken] = new AuthTokenData(false, request.LoggedIn && request.User.MailToken != null);
                    }
                }
            }
            updateDatabase = false;
            return true;
        }
        else
        {
            //invalid, report as a failed attempt
            AccountManager.ReportFailedAuth(request.Context);
            updateDatabase = false;
            return false;
        }
    }

    /// <summary>
    /// Generates a QR code (and converts it to base64) to add the secret key to an authenticator app (works with Google Authenticator) and returns the image source value for HTML.
    /// </summary>
    /// <param name="domain">The domain to be listed in the authenticator app.</param>
    /// <param name="username">The username to be listed in the authenticator app.</param>
    /// <returns></returns>
    public string QRImageBase64Src(string domain, string username)
        => Parsers.QRImageBase64Src($"otpauth://totp/{domain}:{username}?secret={SecretKey}&issuer={domain}");
}