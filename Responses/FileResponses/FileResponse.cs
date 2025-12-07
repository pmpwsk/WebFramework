using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of a file.
/// </summary>
public class FileResponse(string path, bool allowCors, string? timestamp) : AbstractFileResponse(allowCors, timestamp)
{
    private readonly string Path = path;
    
    public bool DeleteAfter = false;

    public override string? Extension
        => new FileInfo(Path).Extension;
    
    public override long? Length
        => new FileInfo(Path).Length;

    protected override async Task WriteTo(HttpContext context)
    {
        await context.Response.SendFileAsync(Path);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        if (DeleteAfter)
            File.Delete(Path);
    }
}