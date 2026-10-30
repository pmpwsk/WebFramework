using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Base = System.Text.Json.JsonSerializer;

namespace uwap.WebFramework.Database;

/// <summary>
/// Serializer using <c>System.Text.Json</c>.
/// </summary>
public class JsonSerializer(JsonSerializerOptions options) : AbstractSerializer
{
    private JsonSerializerOptions Options = options;
    
    public JsonSerializer() : this(new()
    {
        WriteIndented = true,
        IncludeFields = true,
        IgnoreReadOnlyProperties = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false)
        }
    })
    {
    }
    
    public override byte[] Serialize<T>(T obj)
        => Base.SerializeToUtf8Bytes(obj, Options);

    public override T? DeserializeNullable<T>(byte[] json) where T : class
        => DeserializeNullable<T>(new ReadOnlySpan<byte>(json));

    /// <summary>
    /// Deserializes the given JSON as a byte span into an object and returns it.<br/>
    /// If the deserialization failed, null is returned.
    /// <typeparam name="T">The object's type.</typeparam>
    /// </summary>
    public T? DeserializeNullable<T>(ReadOnlySpan<byte> json) where T : class
    {
        try
        {
            return Base.Deserialize<T>(json, Options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Deserializes the given JSON as a byte span into an object and returns it.<br/>
    /// If the deserialization failed, a <c>SerializationException</c> is thrown.
    /// <typeparam name="T">The object's type.</typeparam>
    /// </summary>
    public T Deserialize<T>(ReadOnlySpan<byte> json) where T : class
        => DeserializeNullable<T>(json) ?? throw new SerializationException();
}