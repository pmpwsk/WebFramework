using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

public partial class User
{
    /// <summary>
    /// The user's auth tokens (key) along with their additional data (value).
    /// </summary>
    [DataMember] internal Dictionary<string, AuthTokenData> _AuthTokens = [];

    /// <summary>
    /// The authentication manager object that is associated with this user or null if none have been created yet.
    /// </summary>
    private AuthManager? _AuthManager = null;

    /// <summary>
    /// The authentication manager object that is associated with this user. A new one is created if none have been created yet.
    /// </summary>
    public AuthManager Auth
        => _AuthManager ??= new(this);

    /// <summary>
    /// Contains methods to manage authentication for the associated user.
    /// </summary>
    public class AuthManager(User user) : IEnumerable<KeyValuePair<string, AuthTokenData>>
    {
        /// <summary>
        /// The user this object is associated with.
        /// </summary>
        readonly User User = user;

        /// <summary>
        /// Checks whether the given authentication token exists without checking if it has finished the login process.
        /// </summary>
        public bool Exists(string authToken)
            => User._AuthTokens.ContainsKey(authToken);

        public bool TryGetValue(string authToken, [MaybeNullWhen(false)] out AuthTokenData authTokenData)
            => User._AuthTokens.TryGetValue(authToken, out authTokenData);
        
        public AuthTokenData Get(string authToken)
            => User._AuthTokens[authToken];

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