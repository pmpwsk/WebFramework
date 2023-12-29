using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
    public class AuthManager : IEnumerable<KeyValuePair<string, AuthTokenData>>
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

        public bool TryGetValue(string authToken, [MaybeNullWhen(false)] out AuthTokenData authTokenData)
            => User._AuthTokens.TryGetValue(authToken, out authTokenData);

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
                if ((!User._AuthTokens.ContainsKey(authToken)) && User._AuthTokens.Count >= Server.Config.Accounts.MaxAuthTokens)
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
        /// Removes the given token from the token dictionary, then adds and returns a new token.<br/>
        /// The newly created token will not require 2FA and will not be temporary, so this shouldn't be used for temporary tokens for sessions that aren't fully logged in!
        /// </summary>
        public string Renew(string oldToken)
        {
            User.Lock();
            if (!User._AuthTokens.Remove(oldToken))
            {
                User.UnlockIgnore();
                throw new ArgumentException("The given token isn't present in the token dictionary.");
            }

            string token;
            do token = Parsers.RandomString(64);
            while (Exists(token));
            User._AuthTokens[token] = new AuthTokenData(false, false);
            User.UnlockSave();
            return token;
        }

        /// <summary>
        /// Generates a new authentication token and returns it.<br/>
        /// If the user is using 2FA, the token will still need it before the login process is finished.
        /// </summary>
        public string AddNew(out bool temporary)
        {
            string token;
            do token = Parsers.RandomString(64);
                while (Exists(token));
            bool twoFactor = User.TwoFactor.TOTPEnabled();
            temporary = twoFactor || User.MailToken != null;
            this[token] = new AuthTokenData(twoFactor, temporary);
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
                {
                    User._AuthTokens.Remove(token);
                    if (Server.Config.Log.AuthTokenExpired)
                        Console.WriteLine($"Deleted an expired token for user {User.Id}.");
                }
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

        /// <summary>
        /// Enumerates all values.
        /// </summary>
        public IEnumerator<KeyValuePair<string, AuthTokenData>> GetEnumerator()
        {
            foreach (var kv in User._AuthTokens)
                yield return kv;
        }

        /// <summary>
        /// Enumerates all values.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var kv in User._AuthTokens)
                yield return kv;
        }
    }
}