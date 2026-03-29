using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of a byte array, sent as a download.
/// </summary>
public class ByteArrayDownloadResponse(byte[] bytes, string filename, string? timestamp) : AbstractFileResponse(false, timestamp)
{
    private readonly byte[] Bytes = bytes;
    
    private readonly string Filename = filename;

    public override string? Extension
        => Filename.Contains('.') ? Filename.Remove(0, Filename.LastIndexOf('.')) : null;
    
    public override long? Length
        => Bytes.Length;

    protected override async Task WriteTo(HttpContext context)
    {
        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(Filename);
        context.Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
        await context.Response.BodyWriter.WriteAsync(Bytes);
    }
}