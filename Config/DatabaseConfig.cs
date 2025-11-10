using uwap.Database;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Config
    {
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
            
            /// <summary>
            /// Whether to cache the serialized version of entries.
            /// </summary>
            public static bool CacheEntries { get; set; } = true;

            /// <summary>
            /// The list of nodes in the cluster.
            /// </summary>
            public static List<ClusterNode> Cluster { get; set; } = [];

            /// <summary>
            /// The domain key for the certificate to use when connecting to other nodes in the cluster.
            /// </summary>
            public static string CertificateDomain { get; set; } = "database";
            
            /// <summary>
            /// The time after which a started entry lock expires.
            /// </summary>
            public static TimeSpan LockExpiration { get; set; } = TimeSpan.FromSeconds(2);
            
            /// <summary>
            /// The time to wait before cancelling a request to another node in the cluster.
            /// </summary>
            public static TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMilliseconds(200);
        }
    }
}