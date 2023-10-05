namespace uwap.WebFramework.Accounts;

public static partial class AccountManager
{
    /// <summary>
    /// Settings for the account manager.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Whether the included account stuff should be enabled or disabled for efficiency.<br/>
        /// Default: false
        /// </summary>
        public static bool Enabled = false;

        /// <summary>
        /// The user table (value) for each domain (key). "any" is supported as a fallback.<br/>
        /// Default: empty dictionary
        /// </summary>
        public static Dictionary<string, UserTable> UserTables = new();

        /// <summary>
        /// The path of the two-factor authentication page while logging in.<br/>
        /// Default: /account/2fa
        /// </summary>
        public static string TwoFactorPath = "/account/2fa";

        /// <summary>
        /// The path of the mail verification page while logging in.<br/>
        /// Default: /account/verify
        /// </summary>
        public static string MailVerifyPath = "/account/verify";

        /// <summary>
        /// Paths that are always allowed while the user is in the process of logging in but hasn't finished doing so yet (e.g. when requiring verification or 2FA).<br/>
        /// Default: { /account/logout }
        /// </summary>
        public static string[]? LoginAllowedPaths = new[] {"/account/logout"};

        /// <summary>
        /// The path of the login page.<br/>
        /// Default: /account/login
        /// </summary>
        public static string LoginPath = "/account/login";

        /// <summary>
        /// The allowed maximum amount of authentication tokens for each account. When exceeding this limit, the least recently used token will be deleted.<br/>
        /// Default: 20
        /// </summary>
        public static int MaxAuthTokens = 20;

        /// <summary>
        /// The time after which an unused authentication token expires.<br/>
        /// Default: 90 days
        /// </summary>
        public static TimeSpan TokenExpiration = TimeSpan.FromDays(90);

        /// <summary>
        /// The time after which an authentication token that was just used gets renewed.<br/>
        /// Default: 1 day
        /// </summary>
        public static TimeSpan TokenRenewalAfter = TimeSpan.FromDays(1);

        /// <summary>
        /// Whether to automatically upgrade password hashes to the set default hash settings when users log in and their hash settings doesn't match the default settings.
        /// </summary>
        public static bool AutoUpgradePasswordHashes = true;

        /// <summary>
        /// The domains that should be used as wildcard domains for authentication cookies.<br/>
        /// If you end up removing a domain from this list later on, make sure the cookies with the wildcard are deleted first. They may not be deleted by the browser when the server requests the deletion once the domain is gone from the list, causing the client to get banned for invalid authentication attempts after a few requests.<br/>
        /// Make sure that all servers for the domain and its subdomains should be allowed to see auth tokens!<br/>
        /// If you're using different servers for the domain and different subdomains, make sure that the auth tokens are synchronized between the servers, otherwise they will delete each other's cookies.<br/>
        /// Example: including "uwap.org" here will share auth cookies between uwap.org, mail.uwap.org, notes.uwap.org and so on, but not subsubdomain.subdomain.uwap.org and so on.
        /// </summary>
        public static List<string> WildcardDomains = new();

        /// <summary>
        /// Settings for failed login attempts.
        /// </summary>
        public static class FailedAttempts
        {
            /// <summary>
            /// Whether IPs with too many failed login accounts in a certain time period should be temporarily banned from further login attempts.<br/>
            /// Default: true
            /// </summary>
            public static bool EnableBanning = true;

            /// <summary>
            /// The amount of failed login attempts in the specified time period after which the requesting IP should be temporarily banned from further login attempts.<br/>
            /// Default: 15
            /// </summary>
            public static int Limit = 15;

            /// <summary>
            /// The time after which a login ban is lifted or current failed attempts are cleared.<br/>
            /// Default: 4 hours
            /// </summary>
            public static TimeSpan BanDuration = TimeSpan.FromHours(4);
        }
    }
}