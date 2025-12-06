using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of a byte array.
/// </summary>
public class ByteArrayResponse(byte[] bytes, string? extension, bool allowCors, string? timestamp) : AbstractFileResponse(allowCors, timestamp)
{
    private readonly byte[] Bytes = bytes;
    
    protected override string? Extension { get; } = extension;
    
    protected override long? Length
        => Bytes.Length;

    protected override async Task WriteTo(HttpContext context)
    {
        await context.Response.BodyWriter.WriteAsync(Bytes);
    }
}