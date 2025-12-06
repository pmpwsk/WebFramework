using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The server's configuration.
    /// </summary>
    public static partial class Config
    {
        /// <summary>
        /// The layers of the middleware in order.<br/>
        /// Default: Init, LetsEncrypt, HttpsRedirect, Redirect, File, Auth, Handler<br/>
        /// All default layers can be found under Server.Layers.[NAME]Layer.
        /// </summary>
        public static List<HandlerDelegate> HandlingLayers { get; set; } =
        [
            Layers.DatabaseLayer,
            Layers.DynamicPageLayer,
            Layers.LetsEncryptLayer,
            Layers.HttpsRedirectLayer,
            Layers.RedirectLayer,
            Layers.FileLayer,
            Layers.SystemFilesLayer,
            Layers.AuthLayer,
            Layers.HandlerLayer
        ];

        /// <summary>
        /// Throws an exception "The server does not allow changing this setting during runtime." if the server is running, otherwise nothing happens.
        /// </summary>
        private static void Complain()
        {
            if (Running)
                throw new Exception("The server does not allow changing this setting during runtime.");
        }

        /// <summary>
        /// The "SERVER" response header.
        /// </summary>
        public static string ServerHeader { get; set; } = "uwap.org/wf";

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
        
        private static int? _ClientCertificatePort = null;
        /// <summary>
        /// The port to be used for HTTPS asking for client certificates or null to disable this feature.<br/>
        /// Default: null
        /// </summary>
        public static int? ClientCertificatePort
        {
            get => _ClientCertificatePort;
            set
            {
                Complain();
                _ClientCertificatePort = value;
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
        public static Dictionary<string, string> MimeTypes { get; set; } = new()
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
            {".ttf", "font/ttf"},
            {".otf", "font/otf"},
            //{".svg", "image/svg+xml"}, duplicate!
            {".eot", "application/vnd.ms-fontobject"}
        };

        /// <summary>
        /// List of file types (extensions with a dot in front of them) of which the content should be cached (only those in ../Public and ../Private).
        /// </summary>
        public static List<string> CacheExtensions { get; set; } =
        [
            ".wfpg",
            //web stuff
            ".html", ".css", ".js", ".json",
            //fonts
            ".woff2", ".woff", ".svg", ".eot", ".ttf",
            //other web stuff
            ".ico", ".txt", ".xml"
        ];

        /// <summary>
        /// Dictionary of the cache duration in client browsers (value, in seconds) for each file type (key, with a dot in front of it).<br/>
        /// This doesn't affect the server's cache in any way.<br/>
        /// Contains a few types by default, check out the source code to see them.<br/>
        /// Example: {".html", 0} disables caching HTML files, {".css", 604800} caches CSS files for one week.
        /// </summary>
        public static Dictionary<string, int> BrowserCacheMaxAge { get; set; } = new()
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
        public static Dictionary<int, string> StatusMessages { get; set; } = new()
        {
            {200, "Success."},
            {201, "Created." },
            {301, "Permanent redirect."},
            {302, "Temporary redirect."},
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
            {503, "Service unavailable."},
            {507, "Insufficient storage."}
        };

        /// <summary>
        /// The function that should be called to configure the Kestrel server.
        /// </summary>
        public static Action<KestrelServerOptions>? ConfigureKestrelAction {get; set;} = null;

        /// <summary>
        /// The function that should be called to configure the ASP.NET services.
        /// </summary>
        public static Action<IServiceCollection>? ConfigureServicesAction {get; set;} = null;

        /// <summary>
        /// The function that should be called to configure the web app just before starting it.
        /// </summary>
        public static Action<WebApplication>? ConfigureWebAppAction { get; set; } = null;
        
        /// <summary>
        /// Whether to allow requesting client certificates after the TLS handshake.<br/>
        /// This isn't supported by most clients but would allow client certificate usage on the main HTTPS port.<br/>
        /// Shared WebFramework databases are able to work on the main HTTPS port with this setting enabled, but that comes with a performance penalty.<br/>
        /// Default: false
        /// </summary>
        public static bool EnableDelayedClientCertificates { get; set; } = false;
        
        /// <summary>
        /// Whether to let requests through the points where WebFramework does not have any handlers.<br/>
        /// Default: false
        /// </summary>
        public static bool AllowMoreMiddlewaresIfUnhandled { get; set; } = false;
        
        /// <summary>
        /// The duration after which change watchers for pages expire, causing their page to be reloaded when the connection is re-gained.<br/>
        /// Default: 30 seconds
        /// </summary>
        public static TimeSpan WatcherExpiration { get; set; } = TimeSpan.FromSeconds(30);
    }
}