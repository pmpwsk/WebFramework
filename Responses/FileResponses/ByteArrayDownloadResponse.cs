using Microsoft.AspNetCore.Http;

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
        context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{Filename}\"");
        await context.Response.BodyWriter.WriteAsync(Bytes);
    }
}