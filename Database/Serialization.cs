using System.Runtime.Serialization.Json;

namespace uwap.WebFramework.Database;

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
        DataContractJsonSerializer serializer = new(typeof(T));
        using MemoryStream stream = new();
        serializer.WriteObject(stream, obj);
        byte[] json = stream.ToArray();
        stream.Close();
        return json;
    }

    /// <summary>
    /// Deserializes the given JSON as a byte array into an object and returns it.<br/>
    /// This method may not be used for table values, use <c>Deserialize(id, json)</c> instead.
    /// </summary>
    public static T? Deserialize<T>(byte[] json) where T : class
    {
        var obj = DeserializeInternal<T>(json);

        if (obj is AbstractTableValue)
            throw new Exception("For table values, use Deserialize(id, json) instead.");
        return obj;
    }
    
    /// <summary>
    /// Internal deserialization without type checking or migration.
    /// </summary>
    private static T? DeserializeInternal<T>(byte[] json) where T : class
    {
        try
        {
            DataContractJsonSerializer serializer = new(typeof(T));
            using MemoryStream stream = new(json);
            return (T?)serializer.ReadObject(stream);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes the given JSON as a byte array into a table value, migrates it to the current version (if necessary) and returns it.
    /// </summary>
    public static T? Deserialize<T>(string tableName, string id, byte[] json) where T : AbstractTableValue
    {
        try
        {
            var obj = DeserializeInternal<T>(json);
            if (obj != null)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                obj.AssemblyVersion ??= new Version(0, 0, 0, 0);
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                obj.Files ??= [];
            
                obj.AfterDeserialization(tableName, id, json);
            }
            
            return obj;
        }
        catch
        {
            return null;
        }
    }
}