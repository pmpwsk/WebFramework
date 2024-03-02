namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Config
    {
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
            public static List<string> Domains { get; set; } = [];

            /// <summary>
            /// Whether to complain in the console if the server is unavailable over any of the domains (= no certificates can be requested for those domains).<br/>
            /// Default: false
            /// </summary>
            public static bool MuteUnreachableErrors { get; set; } = false;
        }
    }
}