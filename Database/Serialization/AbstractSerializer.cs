using System.Runtime.Serialization;
using System.Text;

namespace uwap.WebFramework.Database;

/// <summary>
/// Generic serializer and deserializer.
/// </summary>
public abstract class AbstractSerializer
{
    /// <summary>
    /// Serializes the given object and returns the resulting JSON as a byte array.
    /// </summary>
    /// <typeparam name="T">The object's type.</typeparam>
    public abstract byte[] Serialize<T>(T obj);
    
    /// <summary>
    /// Deserializes the given JSON as a byte array into an object and returns it.<br/>
    /// If the deserialization failed, null is returned.
    /// <typeparam name="T">The object's type.</typeparam>
    /// </summary>
    public abstract T? DeserializeNullable<T>(byte[] json) where T : class;
    
    /// <summary>
    /// Deserializes the given JSON as a byte array into an object and returns it.<br/>
    /// If the deserialization failed, a <c>SerializationException</c> is thrown.
    /// <typeparam name="T">The object's type.</typeparam>
    /// </summary>
    public T Deserialize<T>(byte[] json) where T : class
        => DeserializeNullable<T>(json) ?? throw new SerializationException();
    
    /// <summary>
    /// Formats the given object as a string and prints it to the standard output.
    /// <typeparam name="T">The object's type.</typeparam>
    /// </summary>
    public virtual void PrintToConsole<T>(T obj)
        => Console.WriteLine(Encoding.UTF8.GetString(Serialize(obj)));
}