using System.Runtime.Serialization.Json;

namespace uwap.Database;

/// <summary>
/// Serializes and deserializes objects to/from JSON for the database.
/// </summary>
public static class Serialization
{
    /// <summary>
    /// Serializes the given object and returns the resulting JSON as a byte array.
    /// </summary>
    /// <typeparam name="T">The object's type.</typeparam>
    public static byte[] Serialize<T>(T obj)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        using MemoryStream stream = new();
        serializer.WriteObject(stream, obj);
        byte[] json = stream.ToArray();
        stream.Close();
        return json;
    }

    /// <summary>
    /// Deserializes the given JSON as a byte array into an object and returns it.
    /// </summary>
    /// <typeparam name="T">The object's type.</typeparam>
    /// <param name="json"></param>
    public static T Deserialize<T>(byte[] json)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        using MemoryStream stream = new MemoryStream(json);
        T obj = (T)(serializer.ReadObject(stream) ?? throw new Exception("Failed to deserialize the provided JSON."));
        stream.Close();
        return obj;
    }
}