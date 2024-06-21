namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Handles ACME challenges for Let's Encrypt.
        /// </summary>
        public static async Task<bool> LetsEncryptLayer(LayerRequestData data)
        {
            if (Config.AutoCertificate.Email != null && data.Path.StartsWith("/.well-known/acme-challenge/"))
            {
                data.Context.Response.ContentType = "text/plain;charset=utf-8";
                Request request = new(data);
                string url = request.Domain + request.Path;
                if (AutoCertificateTokens.TryGetValue(url, out string? value))
                    await request.Write(value);
                else request.Status = 404;
                await request.Finish();
                return true;
            }

            return false;
        }
    }
}