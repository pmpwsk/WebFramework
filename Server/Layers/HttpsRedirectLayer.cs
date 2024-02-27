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
            if (Config.HttpsPort != null && !data.Context.Request.IsHttps)
            {
                data.Context.Response.Redirect($"https://{data.Domain}{(Config.HttpsPort == 443 ? "" : $":{Config.HttpsPort}")}{data.Path}{data.Context.Request.QueryString}", true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}