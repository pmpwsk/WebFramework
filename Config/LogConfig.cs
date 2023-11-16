namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Config
    {
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
    }
}