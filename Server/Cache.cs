using System.Security.Cryptography;
using System.Text;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The cache dictionary (key = relative path, value = entry data).
    /// </summary>
    internal static Dictionary<string, CacheEntry> Cache = new Dictionary<string, CacheEntry>();

    /// <summary>
    /// Updates the cached metadata for ../Public and ../Private (and file contents for file types that are set to be cached).
    /// </summary>
    private static void UpdateCache()
    {
        //update existing files and remove missing files
        foreach (var entry in Cache)
        {
            string location = $"../{(entry.Value.IsPublic ? "Public" : "Private")}{entry.Key}";
            if (File.Exists(location))
            {
                if (entry.Value.File != null)
                {
                    DateTime modifiedUtc = new FileInfo(location).LastWriteTimeUtc;
                    if (entry.Value.File.ModifiedUtc != modifiedUtc || !entry.Value.File.Check())
                    {
                        Cache[entry.Key].File = new CacheFile(File.ReadAllBytes(location), modifiedUtc);
                    }
                }
            }
            else Cache.Remove(entry.Key);
        }

        //this is necessary to convert the file paths into cache keys
        int parentLength = Directory.GetCurrentDirectory().LastIndexOfAny(new[] {'/', '\\'});
        //add new files from private folder
        if (Directory.Exists("../Private"))
            foreach (string path in Directory.GetFiles("../Private", "*", SearchOption.AllDirectories))
            {
                FileInfo file = new(path);
                string key = file.FullName.Remove(0, parentLength + 9).Replace('\\', '/');
                if (key.StartsWith('.'))
                    continue;
                if (!Cache.ContainsKey(key))
                {
                    if (Config.CacheExtensions.Contains(file.Extension))
                        Cache[key] = new CacheEntry(key, new CacheFile(File.ReadAllBytes(path), file.LastWriteTimeUtc), file.Extension, false);
                    else Cache[key] = new CacheEntry(key, null, file.Extension, false);
                }
            }
        //add new files from public folder
        if (Directory.Exists("../Public"))
            foreach (string directory in Directory.GetDirectories("../Public", "*", SearchOption.TopDirectoryOnly))
                foreach (string path in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                {
                    FileInfo file = new(path);
                    string key = file.FullName.Remove(0, parentLength + 8).Replace('\\', '/');
                    if (key.StartsWith('.'))
                        continue;
                    if ((!Cache.TryGetValue(key, out var entry)) || !entry.IsPublic) //overwrite if private exists
                    {
                        if (Config.CacheExtensions.Contains(file.Extension))
                            Cache[key] = new CacheEntry(key, new CacheFile(File.ReadAllBytes(path), new FileInfo(path).LastWriteTimeUtc), file.Extension, true);
                        else Cache[key] = new CacheEntry(key, null, file.Extension, true);
                    }
                }
    }

    /// <summary>
    /// Contains data about an entry of the cache.
    /// </summary>
    internal class CacheEntry
    {
        /// <summary>
        /// The key in the cache dictionary (relative path).
        /// </summary>
        public string Key;

        /// <summary>
        /// The file content along with metadata about the content or null if the file type isn't being cached.
        /// </summary>
        public CacheFile? File;

        /// <summary>
        /// The file type.
        /// </summary>
        public string Extension;

        /// <summary>
        /// true if the file is in ../Public, false if it is in ../Private.
        /// </summary>
        public bool IsPublic;

        /// <summary>
        /// Creates a new object for data about an entry of the cache.
        /// </summary>
        internal CacheEntry(string key, CacheFile? file, string extension, bool isPublic)
        {
            Key = key;
            File = file;
            Extension = extension;
            IsPublic = isPublic;
        }

        /// <summary>
        /// The usable file path.
        /// </summary>
        public string Path => $"../{(IsPublic?"Public":"Private")}/{Key}";

        /// <summary>
        /// Appends the lines from the file to the given list and adds the given prefix in front of every line.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="linePrefix"></param>
        public void AppendTextLinesTo(List<string> list, string linePrefix = "")
        {
            if (File == null)
                foreach (string line in System.IO.File.ReadAllLines(Path))
                    list.Add(linePrefix + line);
            else
                using (StreamReader reader = new StreamReader(new MemoryStream(File.Content), true))
                    while (!reader.EndOfStream)
                        list.Add(linePrefix + reader.ReadLine());
        }

        /// <summary>
        /// Returns the file's lines as a list, assuming that it is a text file.
        /// </summary>
        public List<string> GetTextLines()
        {
            List<string> list = new();
            AppendTextLinesTo(list);
            return list;
        }

        /// <summary>
        /// The UTC date and time when the file was last modified (according to the cache if the content was cached).
        /// </summary>
        /// <returns></returns>
        public DateTime GetModifiedUtc()
            => File==null ? new FileInfo(Path).LastWriteTimeUtc : File.ModifiedUtc;
    }

    /// <summary>
    /// Contains a file's content and cached metadata about the content.
    /// </summary>
    internal class CacheFile
    {
        /// <summary>
        /// The file's raw content.
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// The MD5 checksum of the file's raw content.
        /// </summary>
        public byte[] Checksum;

        /// <summary>
        /// The UTC date and time when the file was last modified according to the cache.
        /// </summary>
        public DateTime ModifiedUtc;

        /// <summary>
        /// Creates a new object to store a file's content and metadata about the content.
        /// </summary>
        internal CacheFile(byte[] content, DateTime modifiedUtc)
        {
            Content = content;
            Checksum = MD5.HashData(Content);
            ModifiedUtc = modifiedUtc;
        }

        /// <summary>
        /// Returns whether the content matches the checksum.<br/>
        /// This should always be true unless there has been a memory error.
        /// </summary>
        public bool Check()
            => Checksum.SequenceEqual(MD5.HashData(Content));
    }
}