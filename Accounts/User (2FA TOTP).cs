using OtpNet;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

public class TwoFactorTotp
{
    readonly User User;

    internal _TwoFactorTotp Data;

    internal TwoFactorTotp(User user)
    {
        if (user._TwoFactor.TOTP == null)
            throw new Exception("This user doesn't have a TOTP object.");

        User = user;
        Data = user._TwoFactor.TOTP;
    }

    /// <summary>
    /// Whether this two-factor authentication object has been verified.<br/>
    /// If it is not verified, it shouldn't be required to log in.
    /// </summary>
    public bool Verified => Data.Verified;

    /// <summary>
    /// The private/secret key using which two-factor codes are generated.
    /// </summary>
    public string SecretKey => Base32Encoding.ToString(Data.SecretKey);

    /// <summary>
    /// The list of recovery codes that are still available to use.
    /// </summary>
    public ReadOnlyCollection<string> Recovery => Data.Recovery.AsReadOnly();

    /// <summary>
    /// Mark the TOTP 2FA object as verified, enabling it.
    /// </summary>
    public void Verify()
    {
        if (!Data.Verified)
        {
            User.Lock();
            Data.Verified = true;
            User.UnlockSave();
        }
    }

    /// <summary>
    /// Replaces the list of recovery codes with a newly generated list.
    /// </summary>
    public void GenerateNewRecoveryCodes()
    {
        User.Lock();
        Data.Recovery = _TwoFactorTotp.GenerateRecoveryCodes();
        User.UnlockSave();
    }

    /// <summary>
    /// Checks whether the given code is valid right now and hasn't been used before.
    /// </summary>
    /// <param name="request">The current request (used to handle failed attempts and tokens).</param>
    /// <param name="tolerateRecovery">Whether to tolerate the usage of a recovery code.</param>
    public bool Validate(string code, IRequest? request, bool tolerateRecovery)
    {
        //banned?
        if (request != null && AccountManager.IsBanned(request.Context))
        {
            return false;
        }

        //recovery code?
        if (tolerateRecovery && Data.Recovery.Contains(code))
        {
            User.Lock();
            Data.Recovery.Remove(code);
            User.UnlockSave();
            return true;
        }

        //valid and unused?
        if (new Totp(Data.SecretKey).VerifyTotp(code, out long timeStepMatched, new VerificationWindow(1, 1)) && !Data.UsedTimeSteps.Contains(timeStepMatched))
        {
            User.Lock();

            //add to used time stepts (and remove the oldest one if the list is full)
            Data.UsedTimeSteps.Add(timeStepMatched);
            if (Data.UsedTimeSteps.Count > 3) Data.UsedTimeSteps.Remove(Data.UsedTimeSteps.Min());

            //does an auth token even exist? (this should be the case, but you never know)
            if (request != null && request.Cookies.Contains("AuthToken"))
            {
                //get and decode the auth token
                string combinedToken = request.Cookies["AuthToken"];
                string id = combinedToken.Remove(12);
                string authToken = combinedToken.Remove(0, 12);
                if (User.Id == id && User.Auth.Exists(authToken))
                {
                    //renew
                    if (Server.Config.Log.AuthTokenRenewed)
                        Console.WriteLine($"Renewed a token after 2FA for user {User.Id}.");
                    AccountManager.AddAuthTokenCookie(User.Id + User.Auth.Renew(authToken), request.Context, false);
                }
            }
            User.UnlockSave();
            return true;
        }
        else
        {
            //invalid, report as a failed attempt
            if (request != null) AccountManager.ReportFailedAuth(request.Context);
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

[DataContract]
internal class _TwoFactorTotp
{
    [DataMember]
    public bool Verified;

    [DataMember]
    public byte[] SecretKey;

    [DataMember]
    public List<string> Recovery;

    /// <summary>
    /// The 3 most recently used time steps. This is used so a code isn't used twice within its active duration.
    /// </summary>
    [DataMember]
    public List<long> UsedTimeSteps = new();

    public _TwoFactorTotp()
    {
        Verified = false;
        SecretKey = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha1);
        Recovery = GenerateRecoveryCodes();
    }

    internal _TwoFactorTotp(TwoFactorData_Old old)
    {
        Verified = old.Verified;
        SecretKey = old._SecretKey;
        Recovery = old.RecoveryCodes;
    }

    /// <summary>
    /// Generates a new list of recovery codes and returns it.<br/>
    /// As this is a static method, it doesn't replace any recovery code list with the generated one yet.
    /// </summary>
    public static List<string> GenerateRecoveryCodes()
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
}