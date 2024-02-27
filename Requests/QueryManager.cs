using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework;

/// <summary>
/// Manages the query of an IRequest.
/// </summary>
public class QueryManager
{
    /// <summary>
    /// The query object.
    /// </summary>
    private IQueryCollection Query;

    /// <summary>
    /// Creates a new object to manage the query of an IRequest.
    /// </summary>
    public QueryManager(IQueryCollection query)
    {
        Query = query;
    }

    /// <summary>
    /// Gets the value of the query entry with the given key. If no such query entry exists, an exception is thrown.
    /// </summary>
    public string this[string key]
    {
        get
        {
            string? value = Query[key];
            if (value == null) throw new ArgumentException("Query does not contain the provided key.");
            return value;
        }
    }

    /// <summary>
    /// Whether the query contains an entry with the given key.
    /// </summary>
    public bool ContainsKey(string key) => Query.ContainsKey(key);

    /// <summary>
    /// Whether the query contains entries for all of the given keys.
    /// </summary>
    public bool ContainsKeys(params string[] keys) => keys.All(Query.ContainsKey);

    /// <summary>
    /// Returns the value of the query entry with the given key or null if no such entry exists.
    /// </summary>
    public string? TryGet(string key) => Query.TryGetValue(key, out var value) ? ((string?)value)??"" : null;

    /// <summary>
    /// Returns whether the query contains an entry with the given key and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (Query.TryGetValue(key, out var v))
        {
            value = ((string?)v)??"";
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the query contains an entry with the given key and type, and the associated value as an out-argument if true.
    /// </summary>
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (Query.TryGetValue(key, out var sv))
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
}