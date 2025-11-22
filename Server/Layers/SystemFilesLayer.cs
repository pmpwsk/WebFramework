using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        public const string SystemFilesLayerPrefix = "/wf/sys";
        
        /// <summary>
        /// Calls plugin or event handlers.
        /// </summary>
        public static async Task<bool> SystemFilesLayer(LayerRequestData data)
        {
            if (!data.Path.StartsWith(SystemFilesLayerPrefix + '/'))
                return false;
            if (data.Method != "GET")
            {
                data.Context.Response.Headers.Append("Cache-Control", "no-cache, private");
                data.Status = 405;
                await data.Context.Response.WriteAsync(Parsers.StatusMessage(405));
                return true;
            }
            
            var relPath = data.Path[SystemFilesLayerPrefix.Length..];
            byte[]? file = SystemFiles.GetFile(relPath, "", data.Domain);
            string? timestamp = SystemFiles.GetFileVersion(relPath);
            if (file != null && timestamp != null)
            {
                //headers
                if (AddFileHeaders(data.Context, Parsers.Extension(relPath), timestamp))
                    return true;

                //send file
                Request fileRequest = new(data) { CorsDomain = Config.FileCorsDomain };
                try
                {
                    await fileRequest.WriteBytes(file);
                }
                catch (Exception ex)
                {
                    fileRequest.Exception = ex;
                    fileRequest.Status = 500;
                    try { await fileRequest.Finish(); } catch { }
                }
                            
                return true;
            }
            
            return false;
        }
    }
}