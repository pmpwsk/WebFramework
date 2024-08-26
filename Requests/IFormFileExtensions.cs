using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

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

        try
        {
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

            if (totalBytes > limitBytes)
            {
                output.Close();
                File.Delete(path);
                return false;
            }

            return true;
        }
        catch
        {
            try
            {
                File.Delete(path);
            }
            catch { }

            throw;
        }

    }

    /// <summary>
    /// Downloads the file into a memory stream and returns it as a byte array. If the given byte limit is exceeded, the download is cancelled and null is returned.
    /// </summary>
    public static byte[]? ToArray(this IFormFile file, long limitBytes)
    {
        if (file.Length > limitBytes)
            return null;
        using Stream input = file.OpenReadStream();
        try
        {
            if (input.Length > limitBytes)
                return null;
        }
        catch (NotSupportedException) { }
        catch { throw; }

        using MemoryStream output = new();
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

        if (totalBytes > limitBytes)
            return null;

        return output.ToArray();
    }
}