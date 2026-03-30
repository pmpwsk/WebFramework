using Base = System.Runtime.Serialization.Json.DataContractJsonSerializer;

namespace uwap.WebFramework.Database;

/// <summary>
/// Serializer using <c>DataContractSerializer</c>.
/// </summary>
public class DataContractJsonSerializer : AbstractSerializer
{
    public override byte[] Serialize<T>(T obj)
    {
        Base serializer = new(typeof(T));
        using MemoryStream stream = new();
        serializer.WriteObject(stream, obj);
        byte[] json = stream.ToArray();
        stream.Close();
        return json;
    }
    
    public override T? DeserializeNullable<T>(byte[] json) where T : class
    {
        try
        {
            Base serializer = new(typeof(T));
            using MemoryStream stream = new(json);
            var result = (T?)serializer.ReadObject(stream);
            if (result is AbstractTableValue tableValue)
                tableValue.EnsureMinimalTableValue();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
            return null;
        }
    }
}