using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of text chunks.
/// </summary>
public class TextResponse(IEnumerable<string> lines) : AbstractTextResponse
{
    private readonly IEnumerable<string> Lines = lines;

    public TextResponse(string text) : this([text])
    {
    }
    
    public override IEnumerable<string> EnumerateChunks()
    {
        bool anyReturned = false;
        foreach (var line in Lines)
        {
            if (anyReturned)
                yield return "\n";
            else
                anyReturned = true;
            
            yield return line;
        }
    }

    public override Task Respond(Request req, HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, private");
        context.Response.ContentType = "text/plain;charset=utf-8";
        return base.Respond(req, context);
    }
}