using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains the data about a user (everything necessary for basic login functionality, email notifications and settings).
/// </summary>
[DataContract]
public partial class User : ITableValue
{
    /// <summary>
    /// Creates a new user using the given data with a random ID for the given user table.
    /// </summary>
    internal User(string username, string mailAddress, string? password, UserTable users)
    {
        string id;
        do id = Parsers.RandomString(12);
            while (users.ContainsKey(id));
        Id = id;
        SetUsername(username, users, false);
        SetMailAddress(mailAddress, users, false);
        if (password != null)
            Password = new(password);
        MailToken = Parsers.RandomString(10, false, false, true);
    }

    /// <summary>
    /// Constructor to turn an old User object into a new one.
    /// </summary>
    public User(User_Old2 old)
    {
        Id = old.Id;
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

    /// <summary>
    /// This user's ID (should be unique to the user table the user was created for).
    /// </summary>
    [DataMember] public string Id {get; private set;}

    /// <summary>
    /// This user's username.
    /// </summary>
    [DataMember] public string Username { get; private set; } = "";
    /// <summary>
    /// Sets the username or throws an exception if it has an invalid format, another user is using it or equals the current one.
    /// </summary>
    public void SetUsername(string value, UserTable users) => SetUsername(value, users, true);
    /// <summary>
    /// Sets the username or throws an exception if it has an invalid format, another user is using it or if it equals the current one.
    /// </summary>
    /// <param name="announce">Whether to add the username to the cache dictionary that is used to quickly find users by their username.</param>
    private void SetUsername(string value, UserTable users, bool announce)
    {
        if (!AccountManager.CheckUsernameFormat(value))
            throw new Exception("Invalid username format.");
        if (Username == value)
            throw new Exception("The provided username is the same as the old one.");
        if (users.FindByUsername(value) != null)
            throw new Exception("Another user with the provided username already exists.");

        if (announce) Lock();
        if (announce) try { users.Usernames.Remove(Username); } catch { }
        Username = value;
        if (announce) users.Usernames[value] = this;
        if (announce) UnlockSave();
    }

    /// <summary>
    /// Sets the password using the default hashing parameters or throws an exception if its format is invalid or if it equals the current one unless it's allowed.
    /// </summary>
    /// <param name="ignoreRules">Whether to allow passwords with an invalid format.</param>
    /// <param name="allowSame">Whether to allow setting to the password to the current one (this just changes the hashing parameters to the default ones).</param>
    public void SetPassword(string? value, bool ignoreRules = false, bool allowSame = false)
    {
        if (value == null)
        {
            Lock();
            Password = null;
            UnlockSave();
        }
        else
        {
            if ((!ignoreRules) && !AccountManager.CheckPasswordFormat(value))
                throw new Exception("Invalid password format.");
            if ((!allowSame) && ValidatePassword(value, null))
                throw new Exception("The provided password is the same as the old one.");

            Lock();
            Password = new(value);
            UnlockSave();
        }
    }

    /// <summary>
    /// This user's mail address.
    /// </summary>
    [DataMember] private string _MailAddress = "";
    public string MailAddress
        => _MailAddress;
    /// <summary>
    /// Sets the mail address or throws an exception if its format is invalid or another user is using it or if it equals the current one.
    /// </summary>
    public void SetMailAddress(string value, UserTable users) => SetMailAddress(value, users, true);
    /// <summary>
    /// Sets the mail address or throws an exception if its format is invalid or another user is using it or if it equals the current one.
    /// </summary>
    /// <param name="announce">Whether to add the mail address to the cache dictionary that is used to quickly find users by their mail address.</param>
    private void SetMailAddress(string value, UserTable users, bool announce)
    {
        if (!AccountManager.CheckMailAddressFormat(value))
            throw new Exception("Invalid mail address format.");
        if (_MailAddress == value)
            throw new Exception("The provided mail address is the same as the old one.");
        if (users.FindByMailAddress(value) != null)
            throw new Exception("Another user with the provided mail address already exists.");

        if (announce) Lock();
        if (announce) try { users.MailAddresses.Remove(_MailAddress); } catch { }
        _MailAddress = value;
        if (announce) users.MailAddresses[value] = this;
        if (announce) UnlockSave();
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
        set
        {
            Lock();
            _AccessLevel = value;
            UnlockSave();
        }
    }

    /// <summary>
    /// Gets or sets this user's password object.<br/>
    /// Default: null
    /// </summary>
    [DataMember] public Password3? Password {get;private set;} = null;

    /// <summary>
    /// The token for mail verification or null if the mail address has already been verified.
    /// </summary>
    [DataMember] public string? MailToken {get; private set;}

    /// <summary>
    /// Generates, sets and returns a new token for mail verification.
    /// </summary>
    public string SetNewMailToken()
    {
        string token;
        do token = Parsers.RandomString(10, false, false, true);
            while (token == MailToken);
        Lock();
        MailToken = token;
        UnlockSave();
        return token;
    }

    /// <summary>
    /// The date and time of this user's registration (when the user's object was first created).
    /// </summary>
    [DataMember] public DateTime Signup { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks whether the given token is the token for mail verification and sets it to null (=verified) if it's the correct one.
    /// </summary>
    /// <param name="request">The request to handle failed attempts to verify the mail address as login attempts.</param>
    public bool VerifyMail(string token, Request req)
    {
        if (AccountManager.IsBanned(req.Context))
            return false;
        if (MailToken == null)
            throw new Exception("This user's mail address is already verified.");

        if (MailToken == token)
        {
            Lock();
            MailToken = null;
            UnlockSave();

            //does an auth token even exist? (this should be the case, but you never know)
            if (req != null && req.Cookies.TryGetValue("AuthToken", out var combinedToken))
            {
                //get and decode the auth token
                string id = combinedToken.Remove(12);
                string authToken = combinedToken.Remove(0, 12);
                if (Id == id && Auth.TryGetValue(authToken, out var data))
                {
                    //renew
                    if (Server.Config.Log.AuthTokenRenewed)
                        Console.WriteLine($"Renewed a token after mail verification for user {Id}.");
                    AccountManager.AddAuthTokenCookie(Id + Auth.Renew(authToken, data), req.Context, false);
                }
            }
            return true;
        }
        else
        {
            AccountManager.ReportFailedAuth(req.Context);
            return false;
        }
    }

    /// <summary>
    /// Checks whether the given password is correct.
    /// </summary>
    /// <param name="request">The request to handle failed login attempts.</param>
    public bool ValidatePassword(string password, Request? req)
    {
        if (Password == null)
            return false;
        if (req != null && AccountManager.IsBanned(req.Context))
            return false;

        if (Password.Check(password))
        {
            if (Server.Config.Accounts.AutoUpgradePasswordHashes && !Password.MatchesDefault())
            {
                Lock();
                Password = new(password);
                UnlockSave();
            }
            return true;
        }

        if (req != null)
            AccountManager.ReportFailedAuth(req.Context);

        return false;
    }
}