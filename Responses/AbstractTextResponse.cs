namespace uwap.WebFramework.Responses;

/// <summary>
/// An abstract response made up of text chunks.
/// </summary>
public abstract class AbstractTextResponse : AbstractMarkdownPart, IResponse
{
    public virtual async Task Respond(Request req)
    {
        req.WriteTextImmediately = true;
        foreach (var chunk in EnumerateChunks())
            await req.Write(chunk);
        await req.Finish();
    }
}