namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The server's configuration.
    /// </summary>
    public static partial class Config
    {
        public static class Domains
        {
            /// <summary>
            /// Text to be appended to titles (value) of each domain (key, "any" is supported) or null to disable appending anything to titles of the domain.<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> TitleExtensions { get; set; } = new();

            /// <summary>
            /// Copyright names (value) to be used in the footer of each domain (key, "any" is supported) or null to disable the copyright message for the domain.<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> CopyrightNames { get; set; } = new();

            /// <summary>
            /// Dictionary for canonical headers, key domains are presented as mirrors of the value domain.<br/>
            /// This also affects plugins, titles, favicons, copyright messages and file serving as backups in case nothing was found for the actual domain (the general fallback is "any").<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> CanonicalDomains { get; set; } = new();

            /// <summary>
            /// Redirects from the key domains to the value domains.<br/>
            /// Example: {"www.uwap.org", "uwap.org"} to redirect from www.uwap.org/path?query to uwap.org/path?query
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string> Redirect { get; set; } = new();

            /// <summary>
            /// If this is set to true, the canonical origin domain of the requested domain and of "any" may be used to serve files and plugins if no better option was found.<br/>
            /// Default: true
            /// </summary>
            public static bool UseCanonicalForHandling { get; set; } = true;
        }

        /// <summary>
        /// Settings for automatic certificates.
        /// </summary>
        public static class AutoCertificate
        {
            /// <summary>
            /// The email address that should be used to request certificates or null if AutoCertificate should be disabled.<br/>
            /// Default: null
            /// </summary>
            public static string? Email { get; set; } = null;

            /// <summary>
            /// The domains that should always get certificates, in addition to the domains that have been discovered elsewhere.
            /// Default: empty list
            /// </summary>
            public static List<string> Domains { get; set; } = new();

            /// <summary>
            /// Whether to complain in the console if the server is unavailable over any of the domains (= no certificates can be requested for those domains).<br/>
            /// Default: false
            /// </summary>
            public static bool MuteUnreachableErrors { get; set; } = false;
        }

        /// <summary>
        /// Settings to control what types of log entries will appear in the console.
        /// </summary>
        public static class Log
        {
            private static bool _AspNet = false;
            /// <summary>
            /// Whether to output logs from ASP.NET.<br/>
            /// Default: false
            /// </summary>
            public static bool AspNet
            {
                get => _AspNet;
                set
                {
                    Complain();
                    _AspNet = value;
                }
            }

            private static bool _Startup = true;
            /// <summary>
            /// Whether to output neutral logs during server startup.<br/>
            /// Failures will always be shown.<br/>
            /// Default: true
            /// </summary>
            public static bool Startup
            {
                get => _Startup;
                set
                {
                    Complain();
                    _Startup = value;
                }
            }

            /// <summary>
            /// Whether to output logs about auth tokens expiring and expired auth tokens being used.<br/>
            /// Default: false
            /// </summary>
            public static bool AuthTokenExpired { get; set; } = false;

            /// <summary>
            /// Whether to output logs about auth tokens being renewed.<br/>
            /// Default: false
            /// </summary>
            public static bool AuthTokenRenewed { get; set; } = false;
        }

        /// <summary>
        /// Settings to control the database.
        /// </summary>
        public static class Database
        {
            /// <summary>
            /// Whether to write the new JSON to the disk if it doesn't match the old one while loading a table.<br/>
            /// Default: true
            /// </summary>
            public static bool WriteBackOnLoad { get; set; } = true;
        }
    }
}