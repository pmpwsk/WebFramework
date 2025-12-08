using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        public const string SystemFilesLayerPrefix = "/wf/sys";
        
        /// <summary>
        /// Calls plugin or event handlers.
        /// </summary>
        public static Task<IResponse?> SystemFilesLayer(Request req)
            => Task.FromResult(SystemFilesLayerSync(req));
        
        public static IResponse? SystemFilesLayerSync(Request req)
        {
            if (!req.Path.StartsWith(SystemFilesLayerPrefix + '/'))
                return null;
            req.ForceGET();
            
            var relPath = req.Path[SystemFilesLayerPrefix.Length..];
            byte[]? file = SystemFiles.GetFile(relPath, "", req.Domain);
            string? timestamp = SystemFiles.GetFileVersion(relPath);
            if (file != null && timestamp != null)
                //send file
                return new ByteArrayResponse(file, Parsers.Extension(relPath), true, timestamp);
            
            return null;
        }
    }
}