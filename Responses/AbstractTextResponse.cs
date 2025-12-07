using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// An abstract response made up of text chunks.
/// </summary>
public abstract class AbstractTextResponse : AbstractMarkdownPart, IResponse
{
    public virtual async Task Respond(Request req, HttpContext context)
    {
        foreach (var chunk in EnumerateChunks())
            await context.Response.WriteAsync(chunk);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}