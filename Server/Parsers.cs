using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        List<string> domains = [ domain ];
        if (Server.Config.Domains.UseCanonicalForHandling && Server.Config.Domains.CanonicalDomains.TryGetValue(domain, out string? tempOrigin) && tempOrigin != null)
            if (!domains.Contains(tempOrigin))
                domains.Add(tempOrigin);
        if (Server.Config.Domains.UseCanonicalForHandling && Server.Config.Domains.CanonicalDomains.TryGetValue("any", out tempOrigin) && tempOrigin != null)
            if (!domains.Contains(tempOrigin))
                domains.Add(tempOrigin);
        if (!domains.Contains("any"))
            domains.Add("any");
        return domains;
    }

    /// <summary>
    /// Generates a status message for the given HTTP status code.
    /// </summary>
    public static string StatusMessage(int status)
        => ((status<400) ? "Status" : "Error") + " " + status + (Server.Config.StatusMessages.TryGetValue(status, out string? m) ? $": {m}" : "");
    
    public static void SetCorsDomain(this HttpContext context, string? value)
    {
        if (value != null)
            context.Response.Headers.Append("Access-Control-Allow-Origin", value);
    }

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
        => c.Request.Scheme + "://";

    /// <summary>
    /// Host (with port if necessary).
    /// </summary>
    public static string Host(this HttpContext c)
    {
        if (c.Request.Host.Port == null || c.Request.Host.Port == (c.Request.IsHttps ? 443 : 80))
            return c.Request.Host.Host;
        return $"{c.Request.Host.Host}:{c.Request.Host.Port}";
    }
    
    /// <summary>
    /// Host without port.
    /// </summary>
    public static string Domain(this HttpContext c)
    {
        string host = Host(c);
        return host.SplitAtLast(':', out string part1, out string part2) && ushort.TryParse(part2, out _) ? part1 : host;
    }

    /// <summary>
    /// Client IP address or null if unknown.
    /// </summary>
    public static string? IP(this HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress;
        if (ip == null)
            return null;
        if (ip.ToString().StartsWith("::ffff:"))
            return ip.ToString().Replace("::ffff:", "");
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
    /// Returns the MD5 byte array of the given data.
    /// </summary>
    public static byte[] ToMD5(this byte[] data)
        => MD5.HashData(data);
    
    /// <summary>
    /// Returns the SHA256 byte array of the UTF8 representation of the given string.
    /// </summary>
    public static byte[] ToSha256(this string source)
        => SHA256.HashData(Encoding.UTF8.GetBytes(source));
    /// <summary>
    /// Returns the SHA512 byte array of the UTF8 representation of the given string.
    /// </summary>
    public static byte[] ToSha512(this string source)
        => SHA512.HashData(Encoding.UTF8.GetBytes(source));

    /// <summary>
    /// Returns a random string of the given length. Possible characters are lowercase and uppercase letters and digits.
    /// </summary>
    public static string RandomString(int length)
        => RandomString(length, true, true, true);

    /// <summary>
    /// Returns a random string of the given length. Possible character sets are lowercase and uppercase letters and digits, they are individually selected.
    /// </summary>
    public static string RandomString(int length, bool lowercase, bool uppercase, bool digits)
    {
        string sets = "";
        if (lowercase)
            sets += 'a';
        if (uppercase)
            sets += 'A';
        if (digits)
            sets += '1';
        if (sets.Length == 0)
            throw new Exception("At least one character set needs to be selected!");

        StringBuilder result = new();
        while (result.Length < length)
            result.Append(RandomItem(RandomItem(sets) switch
            {
                'a' => "abcdefghijklmnopqrstuvwxyz",
                'A' => "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                '1' => "0123456789",
                _ => throw new Exception("Unrecognized character set!")
            }));
        return result.ToString();
    }

    /// <summary>
    /// Returns a random string of the given length. Possible character sets are lowercase and uppercase letters and digits, they are individually selected.
    /// </summary>
    public static string RandomString(int length, bool lowercase, bool uppercase, bool digits, Func<string, bool> condition)
    {
        string result;
        do result = RandomString(length, lowercase, uppercase, digits);
        while (!condition(result));
        return result;
    }

    /// <summary>
    /// Returns a random string of the given length. Possible characters are lowercase and uppercase letters and digits.
    /// </summary>
    public static string RandomString(int length, Func<string, bool> condition)
        => RandomString(length, true, true, true, condition);
    
    /// <summary>
    /// Returns a random string of the given length. Possible character sets are lowercase and uppercase letters and digits, they are individually selected.
    /// </summary>
    public static string RandomString(int length, bool lowercase, bool uppercase, bool digits, IEnumerable<string> forbiddenValues)
        => RandomString(length, lowercase, uppercase, digits, token => !forbiddenValues.Contains(token));

    /// <summary>
    /// Returns a random string of the given length. Possible characters are lowercase and uppercase letters and digits.
    /// </summary>
    public static string RandomString(int length, IEnumerable<string> forbiddenValues)
        => RandomString(length, true, true, true, forbiddenValues);
    
    /// <summary>
    /// Returns a random string of the given length. Possible character sets are lowercase and uppercase letters and digits, they are individually selected.
    /// </summary>
    public static string RandomString(int length, bool lowercase, bool uppercase, bool digits, string? forbiddenValue)
        => RandomString(length, lowercase, uppercase, digits, token => token != forbiddenValue);

    /// <summary>
    /// Returns a random string of the given length. Possible characters are lowercase and uppercase letters and digits.
    /// </summary>
    public static string RandomString(int length, string? forbiddenValue)
        => RandomString(length, true, true, true, forbiddenValue);

    /// <summary>
    /// Returns a random item from the given array.
    /// </summary>
    public static T RandomItem<T>(T[] values)
        => values[RandomNumberGenerator.GetInt32(values.Length)];

    /// <summary>
    /// Returns a random character from the given string.
    /// </summary>
    public static char RandomItem(string characters)
        => characters[RandomNumberGenerator.GetInt32(characters.Length)];

    /// <summary>
    /// Generates a QR code (with or without borders) of the given text and returns an HTML image source for it in base64.
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
    public static string GetFirstSegment(this Request req, out string rest)
        => GetFirstSegment(req.Path, out rest);

    /// <summary>
    /// Replaces &lt; with &amp;lt; and &gt; with &amp;gt;.
    /// </summary>
    public static string HtmlSafe(this string source)
    {
        StringBuilder text = new();
        foreach (char c in source)
            switch (c)
            {
                case '\r':
                    break;
                case '\n':
                    text.Append("&#10;");
                    break;
                case '\t':
                    text.Append("&#9;");
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
    /// Replaces &lt; with &amp;lt; and &gt; with &amp;gt; if doNotActuallyDoIt is false, otherwise does nothing.
    /// </summary>
    public static string HtmlSafe(this string source, bool doNotActuallyDoIt)
        => doNotActuallyDoIt ? source : source.HtmlSafe();

    /// <summary>
    /// Replaces \n with &#13;&#10; and " with &quot;.
    /// </summary>
    public static string HtmlValueSafe(this string source)
    {
        StringBuilder text = new();
        foreach (char c in source)
            switch (c)
            {
                case '\r':
                    break;
                case '\n':
                    text.Append("&#10;");
                    break;
                case '\t':
                    text.Append("&#9;");
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
            if (dictionary.TryGetValue(key, out var value))
            {
                result = value;
                return true;
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
        result = GetValueAny(dictionary, keys);
        return result != null;
    }

    /// <summary>
    /// Attempts to get the value for one of the given keys in the given dictionary in the order they are provided in.
    /// </summary>
    public static TValue? GetValueAny<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys) where TKey : notnull
    {
        foreach (TKey key in keys)
            if (dictionary.TryGetValue(key, out var value))
                return value;
        
        return default;
    }
    
    /// <summary>
    /// Adds the value to the given key using the given function or returns the existing value.
    /// </summary>
    public static TValue GetValueOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> creator) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        value = creator();
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Capitalizes the first letter of the given string.
    /// </summary>
    public static string CapitalizeFirstLetter(this string value)
        => value == "" ? "" : (value.Remove(1).ToUpper() + value.Remove(0, 1));

    /// <summary>
    /// Returns the part of the given string before the first occurrence of the given separator.
    /// </summary>
    public static string Before(this string value, string separator)
    {
        int index = value.IndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string before the first occurrence of the given separator.
    /// </summary>
    public static string Before(this string value, char separator)
    {
        int index = value.IndexOf(separator);
        if (index == -1)
            return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string after the last occurrence of the given separator.
    /// </summary>
    public static string After(this string value, string separator)
    {
        int index = value.LastIndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return value;
        return value.Remove(0, index + separator.Length);
    }

    /// <summary>
    /// Returns the part of the given string after the last occurrence of the given separator.
    /// </summary>
    public static string After(this string value, char separator)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1)
            return value;
        return value.Remove(0, index + 1);
    }

    /// <summary>
    /// Returns the part of the given string before the last occurrence of the given separator.
    /// </summary>
    public static string BeforeLast(this string value, string separator)
    {
        int index = value.LastIndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string before the last occurrence of the given separator.
    /// </summary>
    public static string BeforeLast(this string value, char separator)
    {
        int index = value.LastIndexOf(separator);
        if (index == -1)
            return value;
        return value.Remove(index);
    }

    /// <summary>
    /// Returns the part of the given string after the first occurrence of the given separator.
    /// </summary>
    public static string AfterFirst(this string value, string separator)
    {
        int index = value.IndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return value;
        return value.Remove(0, index + separator.Length);
    }

    /// <summary>
    /// Returns the part of the given string after the first occurrence of the given separator.
    /// </summary>
    public static string AfterFirst(this string value, char separator)
    {
        int index = value.IndexOf(separator);
        if (index == -1)
            return value;
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
        int index = value.IndexOf(separator, StringComparison.Ordinal);
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
        int index = value.LastIndexOf(separator, StringComparison.Ordinal);
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
        => value[..^count];

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
            if (c is >= '0' and <= '9' or >= 'a' and <= 'z' or '-')
                builder.Append(c);
        return builder.ToString();
    }

    /// <summary>
    /// Returns the file extension of the given path including the preceding dot or an empty string if no file extension was found.
    /// </summary>
    public static string Extension(string path)
    {
        int slash = path.LastIndexOfAny(['/', '\\']);
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
    public static string EnumerationText(this ICollection<string> values)
        => values.Count switch
        {
            0 => "",
            1 => values.First(),
            _ => string.Join(", ", values.SkipLast(1)) + " and " + values.Last(),
        };

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

    /// <summary>
    /// Returns the Base64 encoded version of the given string's UTF8 representation.
    /// </summary>
    public static string ToBase64(this string value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    /// <summary>
    /// Returns the UTF8 string of the bytes decoded from the given Base64 string.
    /// </summary>
    public static string FromBase64(this string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    /// <summary>
    /// Returns the Base64 encoded version of the given string's UTF8 representation, while replacing / with _.
    /// </summary>
    public static string ToBase64PathSafe(this string value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).Replace('/', '_');

    /// <summary>
    /// Returns the UTF8 string of the bytes decoded from the given Base64 string, while replacing _ with /.
    /// </summary>
    public static string FromBase64PathSafe(this string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64.Replace('_', '/')));

    /// <summary>
    /// Formats the version of the given assembly as a string while skipping unnecessary zero segments at the end.<br/>
    /// A.0.0.0 will be formatted as A.0, A.B.0.0 as A.B, A.B.C.0 as A.B.C and A.B.C.D as A.B.C.D.<br/>
    /// If the given assembly doesn't have a version, 0.1 will be returned.
    /// </summary>
    public static string VersionString(Assembly assembly)
    {
        var version = assembly.GetName().Version;
        if (version == null)
            return "0.1";
        if (version.MinorRevision != 0)
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.MinorRevision}";
        if (version.Build != 0)
            return $"{version.Major}.{version.Minor}.{version.Build}";
        return $"{version.Major}.{version.Minor}";
    }

    /// <summary>
    /// Formats the given path (possibly a relative path or including a domain) into a full path with a list of possible domains to match and returns the extracted query string, if present.<br/>
    /// This method is used to format paths in a way that the timestamps for referenced files can be found, even if the references include a domain or are relative paths.
    /// </summary>
    public static void FormatPath(Request req, string pathIn, List<string> domainsIn, out string pathOut, out List<string> domainsOut, out string? queryString)
    {
        queryString = pathIn.SplitAtFirst('?', out pathIn, out queryString) ? '?' + queryString : null;
        if (pathIn.StartsWith('/'))
        {
            domainsOut = domainsIn;
            pathOut = pathIn;
        }
        else if (pathIn.StartsWith("http://"))
        {
            string domainFromPath = pathIn[7..].Before('/');
            domainsOut = Domains(domainFromPath);
            pathOut = pathIn[(7+domainFromPath.Length)..];
        }
        else if (pathIn.StartsWith("https://"))
        {
            string domainFromPath = pathIn[8..].Before('/');
            domainsOut = Domains(domainFromPath);
            pathOut = pathIn[(8+domainFromPath.Length)..];
        }
        else
        {
            domainsOut = domainsIn;
            List<string> segments = [.. req.Path.Split('/')];
            if (segments.Count > 1)
                segments.RemoveAt(segments.Count - 1);
            foreach (string segment in pathIn.Split('/'))
                switch (segment)
                {
                    case ".":
                        continue;
                    case "..":
                        if (segments.Count == 0 || (segments.Count == 1 && segments.First() == ""))
                            segments.Add(segment);
                        else segments.RemoveAt(segments.Count - 1);
                        break;
                    default:
                        segments.Add(segment);
                        break;
                }
            pathOut = string.Join('/', segments);
        }
    }

    /// <summary>
    /// Returns ?[pair] if query is null or an empty string, otherwise &[pair].
    /// </summary>
    public static string QueryStringSuffix(string? query, string pairToAdd)
        => (string.IsNullOrEmpty(query) ? '?' : '&') + pairToAdd;
    
    /// <summary>
    /// Removes the given second-layer item and removes the first-layer key if its collection is empty after that operation. 
    /// </summary>
    public static bool RemoveAndClean<A, B, C>(this Dictionary<A, C> dictionary, A keyA, B keyB) where A : notnull where B : notnull where C : ICollection<B>
    {
        if (dictionary.TryGetValue(keyA, out var subDict))
        {
            bool changed = subDict.Remove(keyB);
            if (subDict.Count == 0)
            {
                changed = true;
                dictionary.Remove(keyA);
            }
            return changed;
        }
        return false;
    }
    
    /// <summary>
    /// Extracts the non-null items in a nullable enumeration.
    /// </summary>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        => enumerable.OfType<T>();
    
    /// <summary>
    /// Maps the items using the given selector and returns non-null results.
    /// </summary>
    public static IEnumerable<R> SelectWhereNotNull<T, R>(this IEnumerable<T> enumerable, Func<T, R?> selector)
        => enumerable.Select(selector).WhereNotNull();
    
    /// <summary>
    /// Applies a function to the "this" parameter to provide cleaner in-line code in niche cases, especially with null checks.<br/>
    /// This allows something like: <c>GetPath()?.Map(p => File.ReadAllBytes(p))</c>
    /// </summary>
    public static R Map<T, R>(this T value, Func<T, R> selector)
        => selector(value);
    
    /// <summary>
    /// Returns the first value in the queue without removing it, or null if the queue is empty.
    /// </summary>
    public static T? PeekOrDefault<T>(this Queue<T> queue)
        => queue.TryPeek(out var value) ? value : default;
    
    /// <summary>
    /// Measures the indentation of the given line by counting the spaces. Tabs are equivalent to the specified number of spaces.
    /// </summary>
    public static (int Indentation, string Remainder) MeasureIndentation(this string line, int tabLength = 4)
    {
        int indentation = 0;
        foreach (var c in line)
        {
            switch (c)
            {
                case ' ':
                    indentation++;
                    break;
                case '\t':
                    indentation += tabLength;
                    break;
                default:
                    return (indentation, line[indentation..]);
            }
        }
        return (line.Length, "");
    }
    
    /// <summary>
    /// Removes all items after the given index, including at the index itself.
    /// </summary>
    public static void RemoveRange<T>(this List<T> list, int index)
    {
        if (index > 0 || list.Count > 0)
            list.RemoveRange(index, list.Count - index);
    }
    
    /// <summary>
    /// Inserts the given value at the given index while removing all elements after it.
    /// </summary>
    public static void ReplaceEnd<T>(this List<T> list, int index, T value)
    {
        list.RemoveRange(index);
        list.Add(value);
    }
    
    /// <summary>
    /// Returns whether the given string contains a segment that is surrounded by the given symbols and returns the first occurrence if one exists.
    /// </summary>
    public static bool ContainsMarkedSegment(this string value, string symbol1, string symbol2, [MaybeNullWhen(false)] out string before, [MaybeNullWhen(false)] out string segment, [MaybeNullWhen(false)] out string after)
    {
        if (value.SplitAtFirst(symbol1, out before, out var afterFirst)
            && afterFirst.SplitAtFirst(symbol2, out segment, out after))
            return true;
        
        before = null;
        segment = null;
        after = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given string contains a segment that is surrounded by the given symbols and returns the first occurrence if one exists.
    /// </summary>
    public static bool ContainsMarkedSegment(this string value, string symbol1, string symbol2, string symbol3, [MaybeNullWhen(false)] out string before, [MaybeNullWhen(false)] out string segment1, [MaybeNullWhen(false)] out string segment2, [MaybeNullWhen(false)] out string after)
    {
        if (value.SplitAtFirst(symbol1, out before, out var afterFirst)
            && afterFirst.SplitAtFirst(symbol2, out segment1, out var afterSecond)
            && afterSecond.SplitAtFirst(symbol3, out segment2, out after))
            return true;
        
        before = null;
        segment1 = null;
        segment2 = null;
        after = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given string is a segment that is surrounded by the given symbols and returns the content if it matches the surroundings.
    /// </summary>
    public static bool IsMarkedSegment(this string value, string symbol1, string symbol2, [MaybeNullWhen(false)] out string segment)
    {
        if (value.StartsWith(symbol1, out var afterFirst)
            && afterFirst.EndsWith(symbol2, out segment))
            return true;
        
        segment = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given string is a segment that is surrounded by the given symbols and returns the content if it matches the surroundings.
    /// </summary>
    public static bool IsMarkedSegment(this string value, string symbol1, string symbol2, string symbol3, [MaybeNullWhen(false)] out string segment1, [MaybeNullWhen(false)] out string segment2)
    {
        if (value.StartsWith(symbol1, out var afterFirst)
            && afterFirst.SplitAtFirst(symbol2, out segment1, out var afterSecond)
            && afterSecond.EndsWith(symbol3, out segment2))
            return true;
        
        segment1 = null;
        segment2 = null;
        return false;
    }
    
    /// <summary>
    /// Returns the top element of the given stack without removing it or <c>null</c> if the stack is empty.
    /// </summary>
    public static T? PeekOrDefault<T>(this Stack<T> stack)
        => stack.TryPeek(out var value) ? value : default;
    
    /// <summary>
    /// Returns whether the given value starts with the given possible beginning and returns the part after it.
    /// </summary>
    public static bool StartsWith(this string value, string start, [MaybeNullWhen(false)] out string rest)
    {
        if (value.StartsWith(start))
        {
            rest = value[start.Length..];
            return true;
        }
        
        rest = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given value starts with any of the given possible beginnings and returns the matched beginning and the part after it.
    /// </summary>
    public static bool StartsWithAny(this string value, [MaybeNullWhen(false)] out string rest, [MaybeNullWhen(false)] out string start, params string[] possibleStarts)
    {
        foreach (var possibleStart in possibleStarts)
            if (value.StartsWith(possibleStart, out rest))
            {
                start = possibleStart;
                return true;
            }
        
        rest = null;
        start = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given value ends with the given possible ending and returns the part before it.
    /// </summary>
    public static bool EndsWith(this string value, string end, [MaybeNullWhen(false)] out string rest)
    {
        if (value.EndsWith(end))
        {
            rest = value[..^end.Length];
            return true;
        }
        
        rest = null;
        return false;
    }
    
    /// <summary>
    /// Returns whether the given value ends with any of the given possible endings and returns the matched ending and the part before it.
    /// </summary>
    public static bool EndsWithAny(this string value, [MaybeNullWhen(false)] out string rest, [MaybeNullWhen(false)] out string end, params string[] possibleEnds)
    {
        foreach (var possibleEnd in possibleEnds)
            if (value.EndsWith(possibleEnd, out rest))
            {
                end = possibleEnd;
                return true;
            }
        
        rest = null;
        end = null;
        return false;
    }
    
    /// <summary>
    /// Returns the configured MIME type for the given URL based on its file extension, or null if the operation failed to find a type.
    /// </summary>
    public static string? GetMimeType(this string url)
    {
        if (url.Before('?').After('/').SplitAtLast('.', out _, out var extension)
            && Server.Config.MimeTypes.TryGetValue('.' + extension, out var mimeType))
            return mimeType;
        return null;
    }
}