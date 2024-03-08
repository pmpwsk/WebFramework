namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Redirects if there's a matching entry in Config.Domains.Redirect.
        /// </summary>
        public static Task<bool> RedirectLayer(LayerRequestData data)
        {
            if (Config.Domains.Redirect.TryGetValue(data.Context.Domain(), out string? redirectTarget))
            {
                data.Redirect(data.Context.Proto() + redirectTarget + data.Path + data.Context.Query(), true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}