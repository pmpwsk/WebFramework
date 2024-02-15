using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Contains elements of one type (must be inheriting ITableValue) and makes them accessible by their associated keys.
/// </summary>
/// <typeparam name="T">The type of the elements that will be stored (must be inheriting ITableValue).</typeparam>
public class Table<T> : ITable, IEnumerable<KeyValuePair<string,T>> where T : ITableValue
{
    /// <summary>
    /// The table's name (shouldn't contain characters that are illegal in the target file system).
    /// </summary>
    public string Name {get; private set;}

    /// <summary>
    /// The dictionary of table entries (value) and their keys (key).<br/>
    /// Default: empty dictionary
    /// </summary>
    protected Dictionary<string, TableEntry<T>> Data = new();

    /// <summary>
    /// Creates a new table object with the given name (nothing else is done).
    /// </summary>
    protected Table(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new table with the given name (shouldn't contain characters that are illegal in the target file system).
    /// </summary>
    protected static Table<T> Create(string name)
    {
        if (!name.All(Tables.KeyChars.Contains)) throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        if (Directory.Exists("../Database/" + name)) throw new Exception("A table with this name already exists, try importing it instead.");
        Directory.CreateDirectory("../Database/" + name);
        Table<T> table = new(name);
        Tables.Dictionary[name] = table;
        return table;
    }

    /// <summary>
    /// Loads or creates the/a table with the given name (shouldn't contain characters that are illegal in the target file system) and returns it.
    /// </summary>
    /// <param name="skipBroken">Whether to skip loading entries that failed to read (otherwise an exception is thrown).</param>
    public static Table<T> Import(string name, bool skipBroken = false)
    {
        if (Tables.Dictionary.TryGetValue(name, out ITable? table)) return (Table<T>)table;
        if (!name.All(Tables.KeyChars.Contains)) throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        if (!Directory.Exists("../Database/" + name)) return Create(name);

        if (Directory.Exists("../Database/Buffer/"+name) && Directory.GetFiles("../Database/Buffer/"+name, "*.json", SearchOption.AllDirectories).Length > 0)
            Console.WriteLine($"The database buffer of table '{name}' contains an entry because a database operation was interrupted. Please manually merge the files and delete the file from the buffer.");

        Table<T> result = new(name);
        result.Reload(skipBroken);
        Tables.Dictionary[name] = result;
        return result;
    }

    /// <summary>
    /// Reloads all entries.
    /// </summary>
    /// <param name="skipBroken">Whether to skip loading entries that failed to read (otherwise an exception is thrown).</param>
    public virtual void Reload(bool skipBroken = false)
    {
        Dictionary<string, TableEntry<T>> data = new();

        foreach (FileInfo file in new DirectoryInfo("../Database/" + Name).EnumerateFiles("*.json"))
        {
            string key = file.Name.Remove(file.Name.Length - 5);
            byte[] json = File.ReadAllBytes(file.FullName);
            try
            {
                T value = Serialization.Deserialize<T>(json);
                if (Server.Config.Database.WriteBackOnLoad)
                {
                    byte[] newJson = Serialization.Serialize(value);
                    if (!newJson.SequenceEqual(json))
                    {
                        File.WriteAllBytes(file.FullName, newJson);
                        json = newJson;
                    }
                }
                data[key] = new TableEntry<T>(Name, key, value, json);
                value.ContainingEntry = data[key];
            }
            catch
            {
                if (!skipBroken) throw new Exception($"Key {key} could not be loaded.");
            }
        }

        Data = data;
    }

    /// <summary>
    /// Transforms this table into a table with elements of the given type using the given function and returns the new table. This table isn't really usable anymore afterwards. It is mainly intended to port a table to a new version of the original element class.
    /// </summary>
    /// <typeparam name="NewType">The type of the new table's elements.</typeparam>
    /// <param name="function">The function that is used to get an object of the new type out of each old object.</param>
    public Table<NewType> Transform<NewType>(Func<T,NewType> function) where NewType : ITableValue
    {
        Table<NewType> result = new(Name);
        foreach (var entry in Data)
        {
            result[entry.Key] = function(entry.Value.Value);
            entry.Value.Value.ContainingEntry = null;
        }
        Data = new Dictionary<string, TableEntry<T>>(); //remove reference for garbage collector
        Tables.Dictionary[Name] = result;
        return result;
    }

    /// <summary>
    /// Renames this table (the new name shouldn't contain any characters that are illegal in the target file system).
    /// </summary>
    public void RenameTable(string newName)
    {
        if (!newName.All(Tables.KeyChars.Contains)) throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        Directory.Move("../Database/"+Name, "../Database/"+ newName);
        Tables.Dictionary[newName] = this;
        Tables.Dictionary.Remove(Name);
        Name = newName;
        foreach (TableEntry<T> entry in Data.Values)
        {
            entry.Table = newName;
        }
    }

    /// <summary>
    /// Deletes this table.
    /// </summary>
    public void DeleteTable()
    {
        Directory.Delete("../Database/"+Name, true);
        foreach (var entry in Data.Values)
            entry.Value.ContainingEntry = null;
        Data = new Dictionary<string, TableEntry<T>>(); //remove reference for garbage collector
        Tables.Dictionary.Remove(Name);
    }

    /// <summary>
    /// Changes the key of the given old key's associated object to the new key.
    /// </summary>
    public void Rename(string oldKey, string newKey)
    {
        if (!newKey.All(Tables.KeyChars.Contains)) throw new Exception($"This key contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        File.Move($"../Database/{Name}/{oldKey}.json", $"../Database/{Name}/{newKey}.json");
        Data[newKey] = Data[oldKey];
        Data[newKey].Key = newKey;
        Data.Remove(oldKey);
    }

    /// <summary>
    /// Removes the entry with the given key and returns true if it existed in the first place, otherwise false.
    /// </summary>
    public virtual bool Delete(string key)
    {
        if (!Data.ContainsKey(key)) return false;

        File.Delete($"../Database/{Name}/{key}.json");
        Data[key].Value.ContainingEntry = null;
        Data.Remove(key);
        return true;
    }

    /// <summary>
    /// Checks whether the entry at the given key matches its saved JSON serialization in memory without attempting to fix errors or checking the JSON serialization on the drive.<br/>
    /// This shouldn't really be trusted as other errors can exist.
    /// </summary>
    public bool Check(string key)
        => Serialization.Serialize(Data[key].Value).SequenceEqual(Data[key].Json);

    /// <summary>
    /// Checks all entries for errors and attempts to fix them.<br/>
    /// If errors are found, more information is written to the console.
    /// </summary>
    public void CheckAndFix()
    {
        foreach (string key in Data.Keys)
        {
            var entry = Data[key];
            entry.CheckAndFix();
        }
    }

    /// <summary>
    /// Gets or sets the value at the given key. When setting values, the current state is automatically being saved.
    /// </summary>
    public virtual T this[string key]
    {
        get => Data[key].Value;
        set
        {
            if (!key.All(Tables.KeyChars.Contains)) throw new Exception($"This key contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
            if (Data.TryGetValue(key, out var entry))
            {
                entry.SetValue(value);
            }
            else
            {
                byte[] json = Serialization.Serialize<T>(value);
                entry = new TableEntry<T>(Name, key, value, json);
                entry.Serialize();
                Data[key] = entry;
                value.ContainingEntry = entry;
            }
        }
    }

    /// <summary>
    /// Checks whether an element with the given key exists.
    /// </summary>
    public bool ContainsKey(string key) => Data.ContainsKey(key);

    /// <summary>
    /// Checks whether an element with the given key exists and returns the found element using the out-argument.
    /// </summary>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        if (Data.TryGetValue(key, out var entry))
        {
            value = entry.Value;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Returns the element with the given key or null if no element with such key exists.
    /// </summary>
    public T? TryGet(string key)
        => Data.TryGetValue(key, out var entry) ? entry.Value : null;

    /// <summary>
    /// Returns all keys as a new list.
    /// </summary>
    public List<string> ListKeys() => Data.Keys.ToList();

    /// <summary>
    /// Returns all values as a new list.
    /// </summary>
    public List<T> ListValues() => Data.Values.Select(x => x.Value).ToList();

    /// <summary>
    /// Enumerates other files that are associated with a given entry while it is locked (for backups).
    /// </summary>
    protected virtual IEnumerable<string> EnumerateOtherFiles(TableEntry<T> entry)
        => [];

    /// <summary>
    /// Enumerates other directories that are associated with a given entry while it is locked (for backups).
    /// </summary>
    protected virtual IEnumerable<string> EnumerateOtherDirectories(TableEntry<T> entry)
        => [];

    public Dictionary<string, Exception> Backup(string id, ReadOnlyCollection<string> basedOnIds, OtherFilesBackupDelegate? otherFilesFunction = null)
    {
        Dictionary<string, Exception> errors = [];
        string dir = $"{Server.Config.Backup.Directory}{id}/{Name}";

        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        foreach (var kv in Data)
        {
            try
            {
                kv.Value.Lock();
                File.Copy($"../Database/{Name}/{kv.Key}.json", $"{dir}/{kv.Key}.json");
                otherFilesFunction?.Invoke(id, basedOnIds, kv.Value);
            }
            catch (Exception ex)
            {
                errors[kv.Key] = ex;
            }
            finally
            {
                kv.Value.UnlockIgnore();
            }
        }

        return errors;
    }

    /// <summary>
    /// Enumerates all values.
    /// </summary>
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        foreach (var kv in Data)
            yield return new KeyValuePair<string,T>(kv.Key, kv.Value.Value);
    }

    /// <summary>
    /// Enumerates all values.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var kv in Data)
            yield return new KeyValuePair<string,T>(kv.Key, kv.Value.Value);
    }
}