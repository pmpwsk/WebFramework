using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for download requests, either to path /dl/... or routed over an app request.
/// </summary>
public class DownloadRequest : IRequest
{
    /// <summary>
    /// Whether something was already sent.
    /// </summary>
    private bool Finished = false;

    /// <summary>
    /// The only origin domain the data gotten from the response should be used for (or null to disable).
    /// </summary>
    public string? CorsDomain
    {
        set
        {
            if (value != null) Context.Response.Headers.Append("Access-Control-Allow-Origin", value);
        }
    }

    /// <summary>
    /// Creates a new download request object with the given context, user, user table and login state.
    /// </summary>
    public DownloadRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
    }

    /// <summary>
    /// Marks the request as finished. If nothing was sent, a status message for code 404 (not found) is written.
    /// </summary>
    public async Task Finish()
    {
        if (Finished) { }
        else
        {
            Finished = true;
            if (Status == 200)
            {
                Status = 404;
            }
            await WriteStatus();
        }
    }

    /// <summary>
    /// Sends the file at the given path as the response. If a file name is provided, that replaces the actual file name.
    /// </summary>
    public async Task SendFile(string path, string? filename = null)
    {
        if (Finished) throw new Exception("Something has already been sent.");
        else
        {
            if (filename == null) filename = new FileInfo(path).Name;
            if (filename.Contains('.'))
            {
                string extension = filename.Remove(0, filename.LastIndexOf('.'));
                if (Server.Config.MimeTypes.ContainsKey(extension)) Context.Response.ContentType = Server.Config.MimeTypes[extension];
            }
            Context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
            await Context.Response.SendFileAsync(path);
            Finished = true;
        }
    }

    /// <summary>
    /// Sends the given byte array as the response with the given file name.
    /// </summary>
    public async Task SendBytes(byte[] bytes, string filename)
    {
        if (Finished) throw new Exception("Something has already been sent.");
        else
        {
            if (filename.Contains('.'))
            {
                string extension = filename.Remove(0, filename.LastIndexOf('.'));
                if (Server.Config.MimeTypes.ContainsKey(extension)) Context.Response.ContentType = Server.Config.MimeTypes[extension];
            }
            Context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
            await Context.Response.BodyWriter.WriteAsync(bytes);
            Finished = true;
        }
    }
}