using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Handles ACME challenges for Let's Encrypt.
        /// </summary>
        public static Task<IResponse?> LetsEncryptLayer(Request req)
            => Task.FromResult(LetsEncryptLayerSync(req));
        
        public static IResponse? LetsEncryptLayerSync(Request req)
        {
            if (Config.AutoCertificate.Email != null && req.Path.StartsWith("/.well-known/acme-challenge/"))
            {
                string url = req.Domain + req.Path;
                if (AutoCertificateTokens.TryGetValue(url, out string? value))
                    return new TextResponse(value);
                else
                    return StatusResponse.NotFound;
            }

            return null;
        }
    }
}