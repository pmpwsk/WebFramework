using System.Runtime.Serialization.Json;
using uwap.WebFramework.Accounts;

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
        DataContractJsonSerializer serializer = new(typeof(T));
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
        if (typeof(T) == typeof(User))
            throw new Exception("For User objects, DeserializeUser(...) should be used instead.");

        DataContractJsonSerializer serializer = new(typeof(T));
        using MemoryStream stream = new(json);
        T obj = (T)(serializer.ReadObject(stream) ?? throw new Exception("Failed to deserialize the provided JSON."));
        stream.Close();
        return obj;
    }

    /// <summary>
    /// Deserializes the given JSON as a byte array into a User and returns it.<br/>
    /// If the serialized object is a User_Old2 object, it will be upgraded and updateDatabase will be true.
    /// </summary>
    /// <param name="updateDatabase">Whether the returned object is different from the originally serialized object (= should be written back to the disk).</param>
    public static User DeserializeUser(byte[] json, out bool updateDatabase)
    {
        DataContractJsonSerializer serializer = new(typeof(User));
        using MemoryStream stream = new(json);
        User result = (User)(serializer.ReadObject(stream) ?? throw new Exception("Failed to deserialize the provided JSON."));
        stream.Close();
        try
        {
            if (result.Username == null)
                throw new Exception("The serialized user object is old and needs to be upgraded.");

            updateDatabase = false;
            return result;
        }
        catch
        {
            updateDatabase = true;
            return new User(Deserialize<User_Old2>(json));
        }
    }
}