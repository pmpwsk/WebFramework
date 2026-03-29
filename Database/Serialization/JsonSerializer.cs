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
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false)
        }
    })
    {
    }
    
    public override byte[] Serialize<T>(T obj)
        => Base.SerializeToUtf8Bytes(obj, Options);

    public override T? DeserializeNullable<T>(byte[] json) where T : class
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
}