using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace uwap.WebFramework.Accounts;

public partial class User
{
    /// <summary>
    /// The user's settings dictionary.
    /// </summary>
    [DataMember] internal Dictionary<string, string> _Settings = [];

    /// <summary>
    /// The settings manager object that is associated with this user or null if none have been created yet.
    /// </summary>
    private SettingsManager? _SettingsManager = null;
    /// <summary>
    /// The settings manager object that is associated with this user. A new one is created if none have been created yet.
    /// </summary>
    public SettingsManager Settings
        => _SettingsManager ??= new(this);

    /// <summary>
    /// Contains methods to manage settings for the associated user.
    /// </summary>
    public class SettingsManager(User user) : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// The user this object is associated with.
        /// </summary>
        readonly User User = user;
        
        /// <summary>
        /// Returns the setting value with the given key.
        /// </summary>
        public string Get(string key)
            => User._Settings[key];

        /// <summary>
        /// Checks whether a setting with the given key exists.
        /// </summary>
        public bool ContainsKey(string key)
            => User._Settings.ContainsKey(key);

        /// <summary>
        /// Lists all keys for settings the user set.
        /// </summary>
        public ReadOnlyCollection<string> ListKeys()
            => User._Settings.Keys.ToList().AsReadOnly();

        /// <summary>
        /// Returns the value of the setting with the given key or null if no such setting exists.
        /// </summary>
        public string? TryGet(string key)
            => User._Settings.GetValueOrDefault(key);

        /// <summary>
        /// Checks whether a setting with the given key exists and returns the value of it using the out-parameter.
        /// </summary>
        /// <param name="value">The value of the setting, if it has been found.</param>
        /// <returns>Whether a setting with the given key has been found.</returns>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
            => User._Settings.TryGetValue(key, out value);

        /// <summary>
        /// Enumerates all values.
        /// </summary>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var kv in User._Settings)
                yield return kv;
        }

        /// <summary>
        /// Enumerates all values.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var kv in User._Settings)
                yield return kv;
        }
    }
}