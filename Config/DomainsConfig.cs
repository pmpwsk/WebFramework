namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Config
    {
        public static class Domains
        {
            /// <summary>
            /// Text to be appended to titles (value) of each domain (key, "any" is supported) or null to disable appending anything to titles of the domain.<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> TitleExtensions { get; set; } = [];

            /// <summary>
            /// Copyright names (value) to be used in the footer of each domain (key, "any" is supported) or null to disable the copyright message for the domain.<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> CopyrightNames { get; set; } = [];

            /// <summary>
            /// Dictionary for canonical headers, key domains are presented as mirrors of the value domain.<br/>
            /// This also affects plugins, titles, favicons, copyright messages and file serving as backups in case nothing was found for the actual domain (the general fallback is "any").<br/>
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string?> CanonicalDomains { get; set; } = [];

            /// <summary>
            /// Redirects from the key domains to the value domains.<br/>
            /// Example: {"www.uwap.org", "uwap.org"} to redirect from www.uwap.org/path?query to uwap.org/path?query
            /// Default: empty dictionary
            /// </summary>
            public static Dictionary<string, string> Redirect { get; set; } = [];

            /// <summary>
            /// If this is set to true, the canonical origin domain of the requested domain and of "any" may be used to serve files and plugins if no better option was found.<br/>
            /// Default: true
            /// </summary>
            public static bool UseCanonicalForHandling { get; set; } = true;
        }
    }
}