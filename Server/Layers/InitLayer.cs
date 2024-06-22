using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Writes the server header, sets the default status code of 200, stops the request if PauseRequests is set to true and sets the list of domains associated with the request.
        /// </summary>
        public static async Task<bool> InitLayer(LayerRequestData data)
        {
            data.Context.Response.Headers.Append("server", Config.ServerHeader);
            data.Context.Response.StatusCode = 200;

            if (PauseRequests)
            {
                data.Context.Response.ContentType = "text/plain;charset=utf-8";
                data.Status = 503;
                data.Context.Response.Headers.RetryAfter = "10";
                await data.Context.Response.WriteAsync("The server is not accepting requests at this time, most likely because it is being updated. Please try again in a few seconds.");
                return true;
            }

            data.Domains = Parsers.Domains(data.Domain);

            return false;
        }
    }
}