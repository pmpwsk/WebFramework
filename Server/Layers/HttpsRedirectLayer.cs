namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Redirects to HTTPS if necessary and possible.
        /// </summary>
        public static Task<bool> HttpsRedirectLayer(LayerRequestData data)
        {
            if (!data.Context.Request.IsHttps)
            {
                var port = Config.HttpsPort ?? Config.ClientCertificatePort;
                if (port != null)
                {
                    data.Context.Response.Redirect(
                        $"https://{data.Domain}{(port == 443 ? "" : $":{port}")}{data.Path}{data.Context.Request.QueryString}",
                        true);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}