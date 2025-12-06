using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of a file.
/// </summary>
public class FileResponse(string path, bool allowCors, string? timestamp) : AbstractFileResponse(allowCors, timestamp)
{
    private readonly string Path = path;
    
    public bool DeleteAfter = false;

    protected override string? Extension
        => new FileInfo(Path).Extension;
    
    protected override long? Length
        => new FileInfo(Path).Length;

    protected override async Task WriteTo(HttpContext context)
    {
        await context.Response.SendFileAsync(Path);
        if (DeleteAfter)
            File.Delete(Path);
    }
}