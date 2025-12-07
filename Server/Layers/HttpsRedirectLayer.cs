using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Redirects to HTTPS if necessary and possible.
        /// </summary>
        public static Task<IResponse?> HttpsRedirectLayer(Request req)
            => Task.FromResult(HttpsRedirectLayerSync(req));
        
        public static IResponse? HttpsRedirectLayerSync(Request req)
        {
            if (!req.IsHttps)
            {
                var port = Config.HttpsPort ?? Config.ClientCertificatePort;
                if (port != null)
                    return new RedirectResponse(
                        $"https://{req.Domain}{(port == 443 ? "" : $":{port}")}{req.Path}{req.Query.FullString}",
                        true
                    );
            }

            return null;
        }
    }
}