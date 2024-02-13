namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Config
    {
        /// <summary>
        /// Settings to control backups.
        /// </summary>
        public static class Backup
        {
            /// <summary>
            /// Whether to run automatic backups.<br/>
            /// Default: false
            /// </summary>
            public static bool Enabled { get; set; } = false;
        }
    }
}