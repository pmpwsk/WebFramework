using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Responses;

/// <summary>
/// A response made up of a file, sent as a download.
/// </summary>
public class FileDownloadResponse(string path, string filename, string? timestamp) : AbstractFileResponse(false, timestamp)
{
    private readonly string Path = path;
    
    private string Filename = filename;
    
    public bool DeleteAfter = false;
    
    public override string? Extension
        => Filename.Contains('.') ? Filename.Remove(0, Filename.LastIndexOf('.')) : null;
    
    public override long? Length
        => new FileInfo(Path).Length;

    protected override async Task WriteTo(HttpContext context)
    {
        context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{Filename}\"");
        await context.Response.SendFileAsync(Path);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        if (DeleteAfter)
            File.Delete(Path);
    }
}