using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using uwap.WebFramework.Database;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Contains a reference to a table value, using the table name and user ID.
/// </summary>
[DataContract]
public class TableReference<T> where T : AbstractTableValue
{
    /// <summary>
    /// The name of the table the value is in.
    /// </summary>
    [JsonInclude]
    [DataMember]
    public readonly string TableName;
    
    /// <summary>
    /// The ID of the value.
    /// </summary>
    [JsonInclude]
    [DataMember]
    public readonly string Id;
    
    /// <summary>
    /// Whether the value has already been loaded.
    /// </summary>
    [JsonIgnore]
    protected bool Loaded;
    
    /// <summary>
    /// The loaded value, if it has been loaded and exists.
    /// </summary>
    [JsonIgnore]
    protected T? CachedValue;

    public TableReference(T value)
    {
        TableName = value.TableName;
        Id = value.Id;
        Loaded = true;
        CachedValue = value;
    }
    
    [JsonConstructor]
    public TableReference(string tableName, string id)
    {
        TableName = tableName;
        Id = id;
        Loaded = false;
        CachedValue = null;
    }
    
    /// <summary>
    /// Whether the given value is the referenced value.
    /// </summary>
    public bool Matches(T value)
        => TableName == value.TableName && Id == value.Id;
    
    /// <summary>
    /// Attempts to retrieve the value for this reference while using a cache.
    /// </summary>
    public async Task<T?> GetNullableAsync()
    {
        if (Loaded)
            return CachedValue;
        
        if (Tables.TryGetTable<Table<T>>(TableName, out var table))
            CachedValue = await table.GetByIdNullableAsync(Id);
        else
            CachedValue = null;
        
        Loaded = true;
        return CachedValue;
    }
    
    /// <summary>
    /// Retrieves the value for this reference while using a cache, or throws an exception if the value couldn't be found.
    /// </summary>
    public async Task<T> GetAsync()
        => await GetNullableAsync() ?? throw new Exception("The value couldn't be found.");

    public override bool Equals(object? obj)
        => obj is TableReference<T> other
           && TableName == other.TableName
           && Id == other.Id;

    public override int GetHashCode()
        => HashCode.Combine(TableName, Id);
}
