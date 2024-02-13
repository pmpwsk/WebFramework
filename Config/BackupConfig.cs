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

            private static string _Directory = "../WFBackups/";
            /// <summary>
            /// The path of the directory that contains all of the backups (for all tables and plugins!) with a succeeding slash.<br/>
            /// Default: "../WFBackups/"
            /// </summary>
            public static string Directory
            {
                get => _Directory;
                set => _Directory = value.EndsWith('/') || value.EndsWith('\\') ? value : (value + '/');
            }
        }
    }
}