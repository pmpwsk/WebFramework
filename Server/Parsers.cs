using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace uwap.WebFramework;

/// <summary>
/// Static helper class to change stuff into text and back.
/// </summary>
public static class Parsers
{
    /// <summary>
    /// Returns a list of all matching domains for a given domain, namely:<br/>
    /// domain, canonical domain of the domain (if present), "any"
    /// </summary>
    public static List<string> Domains(string domain)
    {
        List<string> domains = new()
        {
            domain
        };
        if (Server.Config.Domains.UseCanonicalForHandling && Server.Config.Domains.CanonicalDomains.TryGetValue(domain, out string? tempOrigin) && tempOrigin != null)
            if (!domains.Contains(tempOrigin)) domains.Add(tempOrigin);
        if (Server.Config.Domains.UseCanonicalForHandling && Server.Config.Domains.CanonicalDomains.TryGetValue("any", out tempOrigin) && tempOrigin != null)
            if (!domains.Contains(tempOrigin)) domains.Add(tempOrigin);
        if (!domains.Contains("any")) domains.Add("any");
        return domains;
    }

    /// <summary>
    /// Generates a status message for the given HTTP status code.
    /// </summary>
    public static string StatusMessage(int status)
        => ((status<400) ? "Status" : "Error") + " " + status + (Server.Config.StatusMessages.ContainsKey(status) ? $": {Server.Config.StatusMessages[status]}" : "");

    /// <summary>
    /// Protocol + Host + Path + Query.
    /// </summary>
    public static string ProtoHostPathQuery(this HttpContext c)
        => c.Proto()+c.Host()+c.Path()+c.Query();
    /// <summary>
    /// Protocol + Host + Path.
    /// </summary>
    public static string ProtoHostPath(this HttpContext c)
        => c.Proto()+c.Host()+c.Path();
    /// <summary>
    /// Protocol + Host.
    /// </summary>
    public static string ProtoHost(this HttpContext c)
        => c.Proto()+c.Host();
    /// <summary>
    /// Host + Path + Query.
    /// </summary>
    public static string HostPathQuery(this HttpContext c)
        => c.Host()+c.Path()+c.Query();
    /// <summary>
    /// Host + Path.
    /// </summary>
    public static string HostPath(this HttpContext c)
        => c.Host()+c.Path();
    /// <summary>
    /// Path + Query.
    /// </summary>
    public static string PathQuery(this HttpContext c)
        => c.Path()+c.Query();
    /// <summary>
    /// Protocol.
    /// </summary>
    public static string Proto(this HttpContext c)
        => c.Request.IsHttps?"https://":"http://";
    /// <summary>
    /// Host (with port if necessary).
    /// </summary>
    public static string Host(this HttpContext c)
    {
        if (c.Request.IsHttps)
        {
            if (Server.Config.HttpsPort == 443) return c.Request.Host.Host;
            else return $"{c.Request.Host.Host}:{Server.Config.HttpsPort}";
        }
        else
        {
            if (Server.Config.HttpPort == 80) return c.Request.Host.Host;
            else return $"{c.Request.Host.Host}:{Server.Config.HttpPort}";
        }
    }
    /// <summary>
    /// Host without port.
    /// </summary>
    public static string Domain(this HttpContext c)
    {
        string host = Host(c);
        if (host.SplitAtLast(':', out string part1, out string part2) && ushort.TryParse(part2, out _))
            return part1;
        else return host;
    }

    /// <summary>
    /// Client IP address or null if unknown.
    /// </summary>
    public static string? IP(this HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress;
        if (ip == null) return null;
        if (ip.ToString().StartsWith("::ffff:")) return ip.ToString().Replace("::ffff:", "");
        else return ip.ToString();
    }
    /// <summary>
    /// Path.
    /// </summary>
    public static string Path(this HttpContext c)
        => c.Request.Path;
    /// <summary>
    /// Query.
    /// </summary>
    public static string Query(this HttpContext c)
        => $"{c.Request.QueryString.Value}";

    /// <summary>
    /// Returns the SHA256 byte array of the UTF8 representation of the given string.
    /// </summary>
    public static byte[] ToSha256(this string source)
        => SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(source));
    /// <summary>
    /// Returns the SHA512 byte array of the UTF8 representation of the given string.
    /// </summary>
    public static byte[] ToSha512(this string source)
        => SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(source));

    /// <summary>
    /// Returns a random string of the given length. Possible characters are lowercase and uppercase letters and digits.
    /// </summary>
    public static string RandomString(int length)
    {
        string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string result = "";
        while (result.Length < length)
        {
            result += chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return result;
    }

    /// <summary>
    /// Generates a QR code (with or without borders) of the given text and returns a HTML image source for it in base64.
    /// </summary>
    public static string QRImageBase64Src(string text, bool border = true)
    {
        QRCodeData qr = new QRCodeGenerator().CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        byte[] png = new PngByteQRCode(qr).GetGraphic(20, border);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(string path, out string rest)
    {
        if (path == "")
        {
            rest = "";
            return "";
        }
        string result = path.StartsWith('/') ? path.Remove(0,1) : path;
        int slash = result.IndexOf('/');
        if (slash == -1)
        {
            rest = "";
            return result;
        }
        else
        {
            rest = result.Remove(0, slash);
            return result.Remove(slash);
        }
    }
    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(this AppRequest request, out string rest)
        => GetFirstSegment(request.Path, out rest);
    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(this UploadRequest request, out string rest)
        => GetFirstSegment(request.Path, out rest);
    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(this PostRequest request, out string rest)
        => GetFirstSegment(request.Path, out rest);
    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(this ApiRequest request, out string rest)
        => GetFirstSegment(request.Path.Remove(0,4), out rest);
    /// <summary>
    /// Returns the first segment of a path as well as the remainder, e.g. for /abc/xyz/... it returns "abc" and "/xyz/...".
    /// </summary>
    public static string GetFirstSegment(this DownloadRequest request, out string rest)
        => GetFirstSegment(request.Path.Remove(0,3), out rest);

    /// <summary>
    /// Replaces < with &lt; and > with &gt;.
    /// </summary>
    public static string HtmlSafe(this string source)
    {
        StringBuilder text = new();
        foreach (char c in source)
            switch (c)
            {
                case '\n':
                    text.Append("&#13;&#10;");
                    break;
                case '<':
                    text.Append("&lt;");
                    break;
                case '>':
                    text.Append("&gt;");
                    break;
                default:
                    text.Append(c);
                    break;
            }
        return text.ToString();
    }

    /// <summary>
    /// Replaces \n with &#13;&#10; and " with &quot;.
    /// </summary>
    public static string HtmlValueSafe(this string source)
    {
        StringBuilder text = new();
        foreach (char c in source)
            switch (c)
            {
                case '\n':
                    text.Append("&#13;&#10;");
                    break;
                case '"':
                    text.Append("&quot;");
                    break;
                default:
                    text.Append(c);
                    break;
            }
        return text.ToString();
    }

    /// <summary>
    /// Attempts to get the value for one of the given keys in the given dictionary in the order they are provided in.<br/>
    /// The out-parameter will be the value of the first key that has a value, if any have been found.<br/>
    /// Returns true if a value was found, otherwise false.
    /// </summary>
    public static bool TryGetValueAny<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, [MaybeNullWhen(false)] out TValue result, params TKey[] keys) where TKey : notnull
    {
        foreach (TKey key in keys)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                result = value;
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the value for one of the given keys in the given dictionary in the order they are provided in.<br/>
    /// The out-parameter will be the value of the first key that has a value, if any have been found.<br/>
    /// Returns true if a value was found, otherwise false.
    /// </summary>
    public static bool TryGetValueAny<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, [MaybeNullWhen(false)] out TValue result, IEnumerable<TKey> keys) where TKey : notnull
    {
        foreach (TKey key in keys)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                result = value;
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Capitalizes the first letter of the given string.
    /// </summary>
    public static string CapitalizeFirstLetter(this string value)
    {
        if (value == "") return "";
        string part1 = value.Remove(1);
        string part2 = value.Remove(0, 1);
        return part1.ToUpper() + part2;
    }

    /// <summary>
    /// Returns the part of the given string before the first occurrence of the given separator.
    /// </summary>
    public static string Before(this string value, string separator)
    {
        int index = value.IndexOf(separator);
        if (index == -1) return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string before the first occurrence of the given separator.
    /// </summary>
    public static string Before(this string value, char separator)
    {
        int index = value.IndexOf(separator);
        if (index == -1) return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string after the last occurrence of the given separator.
    /// </summary>
    public static string After(this string value, string separator)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1) return value;
        return value.Remove(0, index + separator.Length);
    }

    /// <summary>
    /// Returns the part of the given string after the last occurrence of the given separator.
    /// </summary>
    public static string After(this string value, char separator)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1) return value;
        return value.Remove(0, index + 1);
    }

    /// <summary>
    /// Splits the string at the first occurrence of the given separator and returns both parts.
    /// </summary>
    public static bool SplitAtFirst(this string value, char separator, out string part1, out string part2)
    {
        int index = value.IndexOf(separator);
        if (index == -1)
        {
            part1 = value;
            part2 = "";
            return false;
        }
        else
        {
            part1 = value.Remove(index);
            part2 = value.Remove(0, index + 1);
            return true;
        }
    }

    /// <summary>
    /// Splits the string at the last occurrence of the given separator and returns both parts.
    /// </summary>
    public static bool SplitAtLast(this string value, char separator, out string part1, out string part2)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1)
        {
            part1 = value;
            part2 = "";
            return false;
        }
        else
        {
            part1 = value.Remove(index);
            part2 = value.Remove(0, index + 1);
            return true;
        }
    }

    /// <summary>
    /// Splits the string at the first occurrence of the given separator and returns both parts.
    /// </summary>
    public static bool SplitAtFirst(this string value, string separator, out string part1, out string part2)
    {
        int index = value.IndexOf(separator);
        if (index == -1)
        {
            part1 = value;
            part2 = "";
            return false;
        }
        else
        {
            part1 = value.Remove(index);
            part2 = value.Remove(0, index + separator.Length);
            return true;
        }
    }

    /// <summary>
    /// Splits the string at the last occurrence of the given separator and returns both parts.
    /// </summary>
    public static bool SplitAtLast(this string value, string separator, out string part1, out string part2)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1)
        {
            part1 = value;
            part2 = "";
            return false;
        }
        else
        {
            part1 = value.Remove(index);
            part2 = value.Remove(0, index + separator.Length);
            return true;
        }
    }

    /// <summary>
    /// Removes the last [count] characters from the string.
    /// </summary>
    public static string RemoveLast(this string value, int count)
        => value.Substring(0, value.Length - count);

    /// <summary>
    /// Returns whether the string starts with any of the given options.
    /// </summary>
    public static bool StartsWithAny(this string value, params string[] starts)
        => starts.Any(value.StartsWith);

    /// <summary>
    /// Returns whether the string ends with any of the given options.
    /// </summary>
    public static bool EndsWithAny(this string value, params string[] ends)
        => ends.Any(value.EndsWith);

    /// <summary>
    /// Removes special characters, replaces spaces with dashes and makes all letters lowercase.
    /// </summary>
    public static string ToId(this string value)
    {
        StringBuilder builder = new();
        foreach (char c in value.Replace(' ', '-').ToLower())
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || c == '-')
                builder.Append(c);
        return builder.ToString();
    }

    /// <summary>
    /// Returns the file extension of the given path including the preceding dot or an empty string if no file extension was found.
    /// </summary>
    public static string Extension(this string path)
    {
        int slash = path.LastIndexOfAny(new char[] { '/', '\\' });
        if (slash != -1)
            path = path.Remove(0, slash + 1);
        int dot = path.LastIndexOf('.');
        if (dot == -1)
            return "";
        else return path.Remove(0, dot);
    }

    /// <summary>
    /// Returns something like this: "[0], [1], [2] and [3]".
    /// </summary>
    public static string EnumerationText(this IEnumerable<string> values)
    {
        return values.Count() switch
        {
            0 => "",
            1 => values.First(),
            _ => string.Join(", ", values.SkipLast(1)) + " and " + values.Last(),
        };
    }

    /// <summary>
    /// Returns the home path of a plugin with the given plugin prefix.<br/>
    /// If the prefix is "", "/" will be returned, otherwise the original prefix will be returned.
    /// </summary>
    public static string PluginHome(this string pluginPrefix)
        => pluginPrefix == "" ? "/" : pluginPrefix;

    
    /// <summary>
    /// Returns the main domain for the given domain.<br/>
    /// Example: for uwap.org, s1.uwap.org, s2.s1.uwap.org etc., uwap.org will be returned.
    /// </summary>
    public static string DomainMain(this string domain)
        => string.Join('.', domain.Split('.').TakeLast(2));
}