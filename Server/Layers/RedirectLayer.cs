using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Redirects if there's a matching entry in Config.Domains.Redirect.
        /// </summary>
        public static Task<IResponse?> RedirectLayer(Request req)
            => Task.FromResult(RedirectLayerSync(req));
        
        public static IResponse? RedirectLayerSync(Request req)
        {
            if (Config.Domains.Redirect.TryGetValue(req.Domain, out string? redirectTarget))
                return new RedirectResponse(req.Proto + redirectTarget + req.Path + req.Query.FullString, true);

            return null;
        }
    }
}