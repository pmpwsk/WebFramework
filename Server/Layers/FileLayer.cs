namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Serves files from the cache.
        /// </summary>
        public static async Task<bool> FileLayer(LayerRequestData data)
        {
            foreach (string key in data.Domains.SelectMany(d => (IEnumerable<string>)[ $"{d}{data.Path}", $"{d}{data.Path}.html" ]))
            {
                try
                {
                    if ((!Cache.TryGetValue(key, out var entry))
                        || (!entry.IsPublic)
                        || data.Path.EndsWith(".html")
                        || data.Path.EndsWith(".wfpg")
                        || key.Split('/').Contains(".."))
                        continue;

                    //don't handle if the file isn't present in the cache and no longer exists
                    if (entry.File == null && !File.Exists($"../Public/{key}"))
                        continue;

                    //headers
                    if (AddFileHeaders(data.Context, entry.Extension, entry.GetModifiedUtc().Ticks.ToString()))
                        return true;

                    //send file
                    ApiRequest request = new(data) { CorsDomain = Config.FileCorsDomain };
                    try
                    {
                        if (entry.File == null) await request.SendFile($"../Public/{key}");
                        else await request.SendBytes(entry.File.Content);
                    }
                    catch (Exception ex)
                    {
                        request.Exception = ex;
                        request.Status = 500;
                        try { await request.Finish(); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    await new ApiRequest(data) { Exception = ex, Status = 500 }.Finish();
                }

                return true;
            }


            return false;
        }
    }
}