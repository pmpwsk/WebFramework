using System.Diagnostics.CodeAnalysis;
using System.Web;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

/// <summary>
/// Manages the query of an IRequest.
/// </summary>
public class QueryManager
{
    public readonly string FullString;
    
    private readonly Dictionary<string, string> Pairs = [];
    
    public QueryManager(string fullString)
    {
        FullString = fullString;
        if (FullString.StartsWith('?'))
        {
            foreach (var pair in FullString[1..].Split('&'))
            {
                if (pair.SplitAtFirst('=', out string key, out string? value))
                {
                    if (value == "")
                        value = null;
                }
                else
                {
                    key = pair;
                    value = null;
                }
                key = HttpUtility.UrlDecode(key);
                value = HttpUtility.UrlDecode(value);
                if (Pairs.TryGetValue(key, out var oldValue) && oldValue != "")
                {
                    if (value != null)
                        Pairs[key] = value + " " + oldValue;
                }
                else if (value == null)
                    Pairs[key] = "";
                else
                    Pairs[key] = value;
            }
        }
    }

    /// <summary>
    /// Whether the query contains an entry with the given key.
    /// </summary>
    public bool ContainsKey(string key)
        => Pairs.ContainsKey(key);
    
    /// <summary>
    /// Attempts to return the value of the query parameter with the given key while forcefully responding with a 400 Bad Request if it doesn't exist.
    /// </summary>
    public string GetOrThrow(string key)
        => TryGet(key) ?? throw new ForcedResponse(StatusResponse.BadRequest);
    
    /// <summary>
    /// Attempts to return the value of the query parameter with the given key while forcefully responding with a 400 Bad Request if it doesn't exist or doesn't match the type.
    /// </summary>
    public T GetOrThrow<T>(string key)
        => TryGet<T>(key) ?? throw new ForcedResponse(StatusResponse.BadRequest);

    /// <summary>
    /// Returns the value of the query entry with the given key or null if no such entry exists.
    /// </summary>
    public string? TryGet(string key)
        => Pairs.GetValueOrDefault(key);

    /// <summary>
    /// Returns the value of the query entry with the given key and type, or null/default if no such entry exists.
    /// </summary>
    public T? TryGet<T>(string key)
        => TryGetValue<T>(key, out var value) ? value : default;

    /// <summary>
    /// Returns whether the query contains an entry with the given key and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => Pairs.TryGetValue(key, out value);

    /// <summary>
    /// Returns whether the query contains an entry with the given key and type, and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (Pairs.TryGetValue(key, out var sv))
        {
            string v = ((string?)sv) ?? "";
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    value = (T)Convert.ChangeType(v, typeof(T));
                    return true;
                case TypeCode.Boolean:
                    {
                        if (bool.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Byte:
                    {
                        if (byte.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Char:
                    if (v.Length == 1)
                    {
                        value = (T)Convert.ChangeType(v[0], typeof(T));
                        return true;
                    }
                    break;
                case TypeCode.DateTime:
                    {
                        if (DateTime.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Decimal:
                    {
                        if (decimal.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Double:
                    {
                        if (double.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Int16:
                    {
                        if (short.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Int32:
                    {
                        if (int.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Int64:
                    {
                        if (long.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.SByte:
                    {
                        if (sbyte.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.Single:
                    {
                        if (float.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.UInt16:
                    {
                        if (ushort.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.UInt32:
                    {
                        if (uint.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                case TypeCode.UInt64:
                    {
                        if (ulong.TryParse(v, out var result))
                        {
                            value = (T)Convert.ChangeType(result, typeof(T));
                            return true;
                        }
                    } break;
                default:
                    throw new Exception("Unrecognized type.");
            }
        }

        value = default;
        return false;
    }
    
    /// <summary>
    /// Lists all query key-value pairs.
    /// </summary>
    public List<KeyValuePair<string,string>> ListAll()
        => Pairs.ToList();
}