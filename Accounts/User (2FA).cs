using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

public partial class User
{
    [DataMember]
    private TwoFactor _TwoFactor = new();

    public TwoFactor TwoFactor
    {
        get => _TwoFactor;
        internal set => _TwoFactor = value;
    }
}

[DataContract]
public class TwoFactor
{
    [DataMember]
    public TwoFactorTOTP? TOTP { get; internal set; }

    public TwoFactor()
    {
        TOTP = null;
    }

    public TwoFactor(TwoFactorData_Old? old)
    {
        if (old == null)
            TOTP = null;
        else
            TOTP = new(old);
    }

    /// <summary>
    /// Returns whether TOTP is enabled and verified.
    /// </summary>
    public bool TOTPEnabled()
        => TOTP != null && TOTP.Verified;

    /// <summary>
    /// Returns whether TOTP is enabled and verified, and the TOTP object if enabled.
    /// </summary>
    public bool TOTPEnabled([MaybeNullWhen(false)] out TwoFactorTOTP totp)
    {
        if (TOTP == null || !TOTP.Verified)
        {
            totp = null;
            return false;
        }

        totp = TOTP;
        return true;
    }

    /// <summary>
    /// Returns whether TOTP has been generated, and the TOTP object if enabled.
    /// </summary>
    public bool TOTPGenerated([MaybeNullWhen(false)] out TwoFactorTOTP totp)
    {
        if (TOTP == null)
        {
            totp = null;
            return false;
        }

        totp = TOTP;
        return true;
    }
}