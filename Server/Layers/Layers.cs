namespace uwap.WebFramework;

/// <summary>
/// Delegate for a layer of the middleware, returns whether the request has been finished (the middleware will not continue if true was returned).
/// </summary>
public delegate Task<bool> LayerDelegate(LayerRequestData data);

public delegate Task<bool> HandlerDelegate(Request req);

public static partial class Server
{
    public static partial class Layers
    {
        public static async Task<bool> Handle(LayerRequestData data, HandlerDelegate handler)
        {
            Request req = new(data);
            try
            {
                if (!await handler(req))
                    return false;
            }
            catch (RedirectSignal redirect)
            {
                try { req.Redirect(redirect.Location, redirect.Permanent); } catch { }
            }
            catch (HttpStatusSignal status)
            {
                try { req.Status = status.Status; } catch { }
            }
            catch (Exception ex)
            {
                req.Exception = ex;
                try { req.Status = 500; } catch { }
            }
            try { await req.Finish(); } catch { }
            
            return true;
        }
    }
}