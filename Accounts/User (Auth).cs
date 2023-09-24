using SmtpServer.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

public partial class User : ITableValue
{
    /// <summary>
    /// The user's auth tokens (key) along with their additional data (value).
    /// </summary>
    [DataMember] private Dictionary<string, AuthTokenData> _AuthTokens = new();

    /// <summary>
    /// The authentication manager object that is associated with this user or null if none have been created yet.
    /// </summary>
    private AuthManager? _AuthManager = null;
    /// <summary>
    /// The authentication manager object that is associated with this user. A new one is created if none have been created yet.
    /// </summary>
    public AuthManager Auth
    {
        get
        {
            _AuthManager ??= new(this);
            return _AuthManager;
        }
    }

    /// <summary>
    /// Contains methods to manage authentication for the associated user.
    /// </summary>
    public class AuthManager
    {
        /// <summary>
        /// The user this object is associated with.
        /// </summary>
        readonly User User;

        /// <summary>
        /// Creates a new authentication manager object for the given user.
        /// </summary>
        public AuthManager(User user)
        {
            User = user;
        }

        /// <summary>
        /// Checks whether the given authentication token exists without checking if it has finished the login process.
        /// </summary>
        public bool Exists(string authToken)
        => User._AuthTokens.ContainsKey(authToken);

        /// <summary>
        /// Gets or sets the additional data for the given authentication token and deletes the least recently used token if the count limit was exceeded.
        /// </summary>
        public AuthTokenData this[string authToken]
        {
            get => User._AuthTokens[authToken];
            set
            {
                User.Lock();

                //find and delete least recently used token if there are too many tokens now
                if ((!User._AuthTokens.ContainsKey(authToken)) && User._AuthTokens.Count >= AccountManager.Settings.MaxAuthTokens)
                {
                    KeyValuePair<string, AuthTokenData>? oldestToken = null;
                    foreach (var token in User._AuthTokens)
                    {
                        if (oldestToken == null || token.Value.Expires < oldestToken.Value.Value.Expires)
                        {
                            oldestToken = token;
                        }
                    }
                    if (oldestToken != null)
                        User._AuthTokens.Remove(oldestToken.Value.Key);
                }

                User._AuthTokens[authToken] = value;
                User.UnlockSave();
            }
        }

        /// <summary>
        /// Generates a new authentication token and returns it.<br/>
        /// If the user is using 2FA, the token will still need it before the login process is finished.
        /// </summary>
        public string AddNew()
        {
            string token;
            do token = Parsers.RandomString(64);
                while (Exists(token));
            bool twoFactor = User.TwoFactor.TOTPEnabled();
            this[token] = new AuthTokenData(twoFactor, twoFactor || User.MailToken != null);
            return token;
        }

        /// <summary>
        /// Deletes the given authentication token if it exists.
        /// </summary>
        public void Delete(string authToken)
        {
            User.Lock();
            if (User._AuthTokens.Remove(authToken))
                User.UnlockSave();
            else User.UnlockIgnore();
        }

        /// <summary>
        /// Deletes all expired authentication tokens.
        /// </summary>
        public void DeleteExpired()
        {
            var affected = User._AuthTokens.Keys.Where(key => User._AuthTokens[key].Expires < DateTime.UtcNow);
            if (affected.Any())
            {
                User.Lock();
                foreach (string token in affected)
                    User._AuthTokens.Remove(token);
                User.UnlockSave();
            }
        }

        /// <summary>
        /// Deletes all authentication tokens except the given one (to log out all other clients).
        /// </summary>
        public void DeleteAllExcept(string authToken)
        {
            var affected = User._AuthTokens.Keys.Where(key => key != authToken);
            if (affected.Any())
            {
                User.Lock();
                foreach (string token in affected)
                    User._AuthTokens.Remove(token);
                User.UnlockSave();
            }
        }

        /// <summary>
        /// Deletes all authentication tokens.
        /// </summary>
        public void DeleteAll()
        {
            if (User._AuthTokens.Any())
            {
                User.Lock();
                User._AuthTokens.Clear();
                User.UnlockSave();
            }
        }

        /// <summary>
        /// Lists all authentication tokens.
        /// </summary>
        public ReadOnlyCollection<string> ListAll()
            => User._AuthTokens.Keys.ToList().AsReadOnly();
    }
}