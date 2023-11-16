namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The server's configuration.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Throws an exception "The server does not allow changing this setting during runtime." if the server is running, otherwise nothing happens.
        /// </summary>
        private static void Complain()
        {
            if (Running) throw new Exception("The server does not allow changing this setting during runtime.");
        }

        /// <summary>
        /// The "SERVER" response header (not meant to be changed, only in forks of the library).
        /// </summary>
        internal static readonly string ServerHeader = "uwap.org/wf";

        private static int? _HttpPort = null;
        /// <summary>
        /// The port to be used for HTTP or null to disable HTTP.<br/>
        /// Intended: 80
        /// Default: null
        /// </summary>
        public static int? HttpPort
        {
            get => _HttpPort;
            set
            {
                Complain();
                _HttpPort = value;
            }
        }
        private static int? _HttpsPort = null;
        /// <summary>
        /// The port to be used for HTTPS or null to disable HTTPS.<br/>
        /// Intended: 443
        /// Default: null
        /// </summary>
        public static int? HttpsPort
        {
            get => _HttpsPort;
            set
            {
                Complain();
                _HttpsPort = value;
            }
        }

        private static int _WorkerInterval = 15;
        /// <summary>
        /// The time to wait before calling the worker again (in minutes).<br/>
        /// 0 disables the worker's regularity (then it is only called when the program starts).<br/>
        /// -1 disables the worker completely, it needs to be manually called if desired.<br/>
        /// Default: 15 (minutes)
        /// </summary>
        public static int WorkerInterval
        {
            get => _WorkerInterval;
            set
            {
                Complain();
                _WorkerInterval = value;
            }
        }

        /// <summary>
        /// The dictionary for known MIME types (key = extension with a dot in front of it, value = full MIME type).<br/>
        /// Contains a few types by default, check out the source code to see them.<br/>
        /// Example: {".html", "text/html"}
        /// </summary>
        public static Dictionary<string, string> MimeTypes { get; set; } = new Dictionary<string, string>()
        {
            //web stuff
            {".html", "text/html"},
            {".css", "text/css"},
            {".js", "text/javascript"},
            {".json", "application/json" },

            //images
            {".ico", "image/x-icon"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"},
            {".svg", "image/svg+xml" },

            //audio
            {".mp3", "audio/mpeg"},

            //video
            {".mp4", "video/mp4"},

            //other files
            {".zip", "application/zip"},
            {".txt", "text/plain"},

            //fonts
            {".woff2", "font/woff2"},
            {".woff", "font/woff"},
            {".tff", "font/ttf"},
            {".otf", "font/otf"},
            //{".svg", "image/svg+xml"}, duplicate!
            {".eot", "application/vnd.ms-fontobject"}
        };

        /// <summary>
        /// List of file types (extensions with a dot in front of them) of which the content should be cached (only those in ../Public and ../Private).
        /// </summary>
        public static List<string> CacheExtensions { get; set; } = new List<string>
        {
            ".wfpg",
            //web stuff
            ".html", ".css", ".js", ".json",
            //fonts
            ".woff2", ".woff", ".svg", ".eot", ".ttf",
            //other web stuff
            ".ico", ".txt", ".xml"
        };

        /// <summary>
        /// Dictionary of the cache duration in client browsers (value, in seconds) for each file type (key, with a dot in front of it).<br/>
        /// This doesn't affect the server's cache in any way.<br/>
        /// Contains a few types by default, check out the source code to see them.<br/>
        /// Example: {".html", 0} disables caching HTML files, {".css", 604800} caches CSS files for one week.
        /// </summary>
        public static Dictionary<string, int> BrowserCacheMaxAge { get; set; } = new Dictionary<string, int>
        {
            {".html", 0},
            {".css", 604800}, //1 week
            {".js", 604800},

            {".png", 604800},
            {".jpg", 604800},
            {".jpeg", 604800},
            {".gif", 604800},
            {".bmp", 604800},

            {".ico", 14400}, //4 hours

            {".woff2", 604800},
            {".woff", 604800},
            {".svg", 604800},
            {".eot", 604800},
            {".ttf", 604800}
        };

        /// <summary>
        /// The CORS domain to be used in responses that serve files from ../Public.<br/>
        /// Browsers will only allow the file to be used in pages of the set domain.
        /// </summary>
        public static string? FileCorsDomain { get; set; } = null;

        /// <summary>
        /// The dictionary of known status messages for HTTP status codes.<br/>
        /// Contains the most common codes by default, check out the source code to see them.<br/>
        /// Example: {404, "Not found."}
        /// </summary>
        public static Dictionary<int, string> StatusMessages { get; set; } = new Dictionary<int, string>()
        {
            {200, "Success."},
            {201, "Created." },
            {304, "Not changed."},
            {400, "Bad request."},
            {401, "Not authenticated."},
            {403, "Forbidden."},
            {404, "Not found."},
            {405, "Method not allowed."},
            {413, "Payload too large."},
            {418, "I'm a teapot."},
            {429, "Too many requests."},
            {500, "Internal server error."},
            {501, "Not implemented."},
            {507, "Insufficient storage."}
        };

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