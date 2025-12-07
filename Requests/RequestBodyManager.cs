using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace uwap.WebFramework;

/// <summary>
/// Manages the body of a request.
/// </summary>
public class RequestBodyManager(HttpContext context)
{
    private HttpContext HttpContext = context;

    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public async Task<string> GetText()
    {
        using StreamReader reader = new(HttpContext.Request.Body, true);
        try
        {
            return await reader.ReadToEndAsync();
        }
        finally
        {
            reader.Close();
        }
    }

    /// <summary>
    /// The request body, interpreted as bytes.
    /// </summary>
    public async Task<byte[]> GetBytes()
    {
        using MemoryStream target = new();
        await HttpContext.Request.Body.CopyToAsync(target);
        return target.ToArray();
    }
}