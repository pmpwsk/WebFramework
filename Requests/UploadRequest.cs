using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for upload requests (POST requests with files).
/// </summary>
public class UploadRequest : TextRequest
{
    /// <summary>
    /// The only origin domain the data gotten from the response should be used for (or null to disable).
    /// </summary>
    public string? CorsDomain
    {
        set
        {
            if (value != null) Context.Response.Headers.Add("Access-Control-Allow-Origin", value);
        }
    }

    /// <summary>
    /// Creates a new upload request object with the given context, user, user table and login state.
    /// </summary>
    public UploadRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
    }

    /// <summary>
    /// The uploaded files.
    /// </summary>
    public IFormFileCollection Files => Context.Request.Form.Files;
}

/// <summary>
/// Extension methods for uploaded files.
/// </summary>
public static class IFormFileExtensions
{
    /// <summary>
    /// Downloads the file to the given path. If the given byte limit is exceeded, the download is cancelled and false is returned, otherwise true.
    /// </summary>
    public static bool Download(this IFormFile file, string path, long limitBytes)
    {
        if (file.Length > limitBytes) return false;
        using Stream input = file.OpenReadStream();
        try
        {
            if (input.Length > limitBytes) return false;
        }
        catch (NotSupportedException) { }
        catch { throw; }

        using Stream output = File.Create(path);
        byte[] buffer = new byte[32768];
        long totalBytes = 0;
        int lastBytes;

        while (totalBytes <= limitBytes)
        {
            lastBytes = input.Read(buffer, 0, (int)Math.Min(buffer.Length, limitBytes - totalBytes));
            if (lastBytes <= 0) break;
            totalBytes += lastBytes;
            if (totalBytes <= limitBytes) output.Write(buffer, 0, lastBytes);
        }

        input.Close();
        output.Close();

        if (totalBytes > limitBytes)
        {
            File.Delete(path);
            return false;
        }

        return true;
    }
}