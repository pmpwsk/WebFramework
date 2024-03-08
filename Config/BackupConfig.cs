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

            /// <summary>
            /// The day of the week on which a full backup should be created, instead of making it based on the previous day's backup.<br/>
            /// Default: Sunday
            /// </summary>
            public static DayOfWeek FreshDay { get; set; } = DayOfWeek.Sunday;

            /// <summary>
            /// The time for backups as an offset from midnight (00:00) UTC.<br/>
            /// Default: 0 (midnight UTC).
            /// </summary>
            public static TimeSpan Time { get; set; } = TimeSpan.FromHours(0);
        }
    }
}