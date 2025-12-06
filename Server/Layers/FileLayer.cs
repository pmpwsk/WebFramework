using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Serves files from the cache.
        /// </summary>
        public static Task<IResponse?> FileLayer(Request req)
            => Task.FromResult(FileLayerSync(req));
        
        public static IResponse? FileLayerSync(Request req)
        {
            if (req.Method != "GET")
                return null;

            foreach (string key in req.Domains.SelectMany(d => (IEnumerable<string>)[ $"{d}{req.Path}", $"{d}{req.Path}.html" ]))
                if (Cache.TryGetValue(key, out var entry)
                    && entry.IsPublic
                    && !req.Path.EndsWith(".html")
                    && !req.Path.EndsWith(".wfpg")
                    && !req.Path.EndsWith(".wfmd")
                    && !key.Split('/').Contains(".."))
                {
                    if (entry.File != null)
                        return new ByteArrayResponse(entry.File.Content, entry.Extension, true,
                            entry.GetModifiedUtc().Ticks.ToString());
                    else if (File.Exists($"../Public/{key}"))
                        return new FileResponse($"../Public/{key}", true, entry.GetModifiedUtc().Ticks.ToString());
                }
            
            return null;
        }
    }
}