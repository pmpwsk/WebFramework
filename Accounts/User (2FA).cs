using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

public partial class User : ITableValue
{
    [DataMember]
    internal _TwoFactor _TwoFactor = new();

    private TwoFactor? _TwoFactorWrapper = null;

    public TwoFactor TwoFactor
    {
        get
        {
            _TwoFactorWrapper ??= new(this);
            return _TwoFactorWrapper;
        }
    }
}

public class TwoFactor
{
    readonly User User;

    public TwoFactorTotp? TOTP { get; private set; } = null;

    internal TwoFactor(User user)
    {
        User = user;
        if (user._TwoFactor.TOTP != null)
            TOTP = new(user);
    }

    /// <summary>
    /// Returns whether TOTP is enabled and verified.
    /// </summary>
    public bool TOTPEnabled()
        => TOTP != null && TOTP.Verified;

    /// <summary>
    /// Returns whether TOTP is enabled and verified, and the TOTP object if enabled.
    /// </summary>
    public bool TOTPEnabled([MaybeNullWhen(false)] out TwoFactorTotp totp)
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
    /// Creates a new object to save two-factor authentication data for an account using a new random secret key and new recovery keys. It needs to be verified before it can be used.
    /// </summary>
    public void GenerateTOTP()
    {
        User.Lock();
        User._TwoFactor.TOTP = new();
        TOTP = new(User);
        User.UnlockSave();
    }

    /// <summary>
    /// Disables TOTP 2FA and deletes the corresponding object.
    /// </summary>
    public void DisableTOTP()
    {
        User.Lock();
        User._TwoFactor.TOTP = null;
        TOTP = null;
        User.UnlockSave();
    }
}

[DataContract]
internal class _TwoFactor
{
    [DataMember]
    public _TwoFactorTotp? TOTP;

    public _TwoFactor()
    {
        TOTP = null;
    }

    public _TwoFactor(TwoFactorData_Old? old)
    {
        if (old == null)
            TOTP = null;
        else
            TOTP = new(old);
    }
}