using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace uwap.WebFramework;

/// <summary>
/// Intended for POST requests (with or without forms, possibly with files).
/// </summary>
public class PostRequest(LayerRequestData data) : SimpleResponseRequest(data)
{
    /// <summary>
    /// The largest allowed request body size for this request in bytes. This may only be set once and only before any reading has begun.
    /// </summary>
    public long? BodySizeLimit
    {
        set => (Context.Features.Get<IHttpMaxRequestBodySizeFeature>() ?? throw new Exception("IHttpMaxRequestBodySizeFeature is not supported.")).MaxRequestBodySize = value;
    }

    /// <summary>
    /// Whether the request has set a content type for a form.
    /// </summary>
    public bool IsForm
        => Context.Request.HasFormContentType;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form
        => Context.Request.Form;

    /// <summary>
    /// The uploaded files.
    /// </summary>
    public IFormFileCollection Files
        => Context.Request.Form.Files;

    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public async Task<string> GetBodyText()
    {
        using StreamReader reader = new(Context.Request.Body, true);
        try
        {
            return await reader.ReadToEndAsync();
        }
        finally
        {
            reader.Close();
            reader.Dispose();
        }
    }
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
        if (file.Length > limitBytes)
            return false;
        using Stream input = file.OpenReadStream();
        try
        {
            if (input.Length > limitBytes)
                return false;
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
            if (lastBytes <= 0)
                break;
            totalBytes += lastBytes;
            if (totalBytes <= limitBytes)
                output.Write(buffer, 0, lastBytes);
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