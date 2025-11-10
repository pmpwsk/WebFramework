using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains the data about a user (everything necessary for basic login functionality, email notifications and settings).
/// </summary>
[DataContract]
public partial class User : AbstractTableValue
{
    /// <summary>
    /// Creates a new user using the given data.
    /// </summary>
    internal User(string username, string mailAddress, string? password)
    {
        Username = username;
        _MailAddress = mailAddress;
        if (password != null)
            Password = new(password);
        MailToken = Parsers.RandomString(10, false, false, true);
    }

    /// <summary>
    /// This user's username.
    /// </summary>
    [DataMember] public string Username {get; internal set;}

    /// <summary>
    /// This user's mail address.
    /// </summary>
    [DataMember] private string _MailAddress;
    public string MailAddress
    {
        get => _MailAddress;
        internal set => _MailAddress = value;
    }

    /// <summary>
    /// This user's access level (for different roles, highest-level administrators should be ushort.MaxValue, 0 is reserved for users that aren't logged in).<br/>
    /// Default: 1
    /// </summary>
    [DataMember] private ushort _AccessLevel = 1;
    /// <summary>
    /// Gets or sets this user's access level (for different roles, highest-level administrators should be ushort.MaxValue, 0 is reserved for users that aren't logged in).<br/>
    /// Default: 1
    /// </summary>
    public ushort AccessLevel
    {
        get => _AccessLevel;
        internal set => _AccessLevel = value;
    }

    /// <summary>
    /// Gets or sets this user's password object.<br/>
    /// Default: null
    /// </summary>
    [DataMember] public Password3? Password {get; internal set;} = null;

    /// <summary>
    /// The token for mail verification or null if the mail address has already been verified.
    /// </summary>
    [DataMember] public string? MailToken {get; internal set;}

    /// <summary>
    /// The date and time of this user's registration (when the user's object was first created).
    /// </summary>
    [DataMember] public DateTime Signup { get; private set; } = DateTime.UtcNow;

    protected override void Migrate(string tableName, string id, byte[] serialized)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Username != null)
            return;
        
        var old = Serialization.Deserialize<User_Old2>(serialized) ?? throw new SerializationException();
        Username = old._Username;
        _MailAddress = old._MailAddress;
        _AccessLevel = old._AccessLevel;
        Password = old.Password==null ? null : new(old.Password);
        MailToken = old.MailToken;
        Signup = old.Signup;
        _TwoFactor = new(old._TwoFactor);
        _AuthTokens = old._AuthTokens;
        _Settings = old._Settings;
    }
}