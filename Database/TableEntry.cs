using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Contains one value for a table along with additional data about it (table, key, serialization, locking information).
/// </summary>
/// <typeparam name="T">The type of values that will be saved here.</typeparam>
public class TableEntry<T> : ITableEntry where T : ITableValue
{
    /// <summary>
    /// The name of the containing table.
    /// </summary>
    public string Table;

    /// <summary>
    /// The associated key to this entry.
    /// </summary>
    public string Key;

    /// <summary>
    /// The stored value.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// The JSON serialization as a byte array that's saved in memory.
    /// </summary>
    public byte[] Json { get; private set; }

    /// <summary>
    /// The HttpContext that is currently locking this entry or null if it is not locked or no context was found while locking.
    /// </summary>
    private HttpContext? LockingContext = null;

    /// <summary>
    /// The amount of times this entry is locked by its current locker or 0 if it's not locked.
    /// </summary>
    private int LockCount = 0;

    /// <summary>
    /// Creates a new object to store one value for a table along with additional data about it (table, key, serialization, locking information).
    /// </summary>
    /// <param name="table">The containing table's name.</param>
    /// <param name="key">The associated key.</param>
    /// <param name="value">The stored value.</param>
    /// <param name="json">The value's JSON serialization.</param>
    public TableEntry(string table, string key, T value, byte[] json)
    {
        Table = table;
        Key = key;
        Value = value;
        Json = json;
    }

    /// <summary>
    /// Unlocks the object after the locking HttpContext is done (as long as it is still locking it and the same context).
    /// </summary>
    /// <param name="obj">The HttpContext that is calling this method because it's done.</param>
    private Task RequestCompleted(object obj)
    {
        HttpContext context = (HttpContext)obj;
        if (LockingContext == context)
        {
            Console.WriteLine("A request was completed without unlocking a locked object. It will be restored to its previous state.");
            try
            {
                Unlock(false, true);
            }
            catch { }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Locks the entry.
    /// <returns>Whether the entry was locked using the current context or without a context at all.</returns>
    /// </summary>
    public bool Lock()
    {
        var context = Server.CurrentHttpContext;
        Lock(context);
        return context != null;
    }
    /// <summary>
    /// Locks the entry to the given context (may be null).
    /// </summary>
    private void Lock(HttpContext? context)
    {
        if (context != null && context == LockingContext)
        {
            LockCount++;
            return;
        }
        while (LockCount > 0 || LockingContext != null)
        {
            Thread.Sleep(50);
        }
        LockCount = 1;
        if (context != null)
        {
            LockingContext = context;
            context.Response.OnCompleted(RequestCompleted, context);
        }
        CheckAndFix(false);
    }

    /// <summary>
    /// Makes the entry's state persistent and unlocks it.
    /// </summary>
    public void UnlockSave()
        => Unlock(true);

    /// <summary>
    /// Restores the entry to the last persistent state and unlocks it.
    /// </summary>
    public void UnlockRestore()
        => Unlock(false);

    /// <summary>
    /// Unlocks the entry while ignoring any changes to it (no saving or restoring).
    /// </summary>
    public void UnlockIgnore()
        => Unlock(null);
    /// <summary>
    /// Unlocks the entry.
    /// </summary>
    /// <param name="save">true to save, false to restore, null to ignore changes.</param>
    /// <param name="forceZero">Whether to set the lock count to 0 instead of lowering it by 1.</param>
    private void Unlock(bool? save, bool forceZero = false)
    {
        if (LockingContext == null)
        {
            if (LockCount == 0)
                throw new Exception("This object is not locked.");
            else if (Server.CurrentHttpContext != null)
                throw new Exception("This object is locked by a request.");
        }
        else
        {
            if (LockingContext != Server.CurrentHttpContext)
                throw new Exception("This object is locked by another request.");
        }

        if (save != null)
        {
            if (save.Value)
                Serialize();
            else Restore();
        }

        if (forceZero)
        {
            LockCount = 0;
            LockingContext = null;
        }
        else
        {
            LockCount--;
            if (LockCount == 0)
                LockingContext = null;
        }
    }

    /// <summary>
    /// Checks the entry for errors and attempts to fix them.<br/>
    /// If errors are found, more information is written to the console.
    /// </summary>
    public void CheckAndFix(bool lockFirst = true)
    {
        try
        {
            if (lockFirst) Lock();
            byte[] objectJson;
            try
            {
                //attempt to serialize the object
                objectJson = Serialization.Serialize(Value);
            }
            catch
            {
                //object broken
                Restore();
                Console.WriteLine($"Error found in table {Table} at key {Key}, the object could not be serialized, possibly because it was corrupted by a memory error. The object has been reverted to the last known good state, so it is no longer up to date.");
                return;
            }

            if (objectJson.SequenceEqual(Json))
            {
                //no issue :)
                return;
            }
            byte[] fileJson = File.ReadAllBytes($"../Database/{Table}/{Key}.json");
            if (objectJson.SequenceEqual(fileJson))
            {
                //json in memory is broken, restoring from file
                Json = fileJson;
                Console.WriteLine($"Error found in table {Table} at key {Key}, the serialization in memory was wrong, possibly because it was corrupted by a memory error. This issue has been fixed automatically.");
            }
            else if (Json.SequenceEqual(fileJson))
            {
                //object is broken, restoring from memory
                Restore();
                Console.WriteLine($"[IMPORTANT] Error found in table {Table} at key {Key}, the object did not match the serialization, possibly because the object did not report a change. The object has been reverted to the last known good state, so it is no longer up to date.");
            }
            else
            {
                //big error! can't be fixed
                Console.WriteLine($"[VERY IMPORTANT!] Error found in table {Table} at key {Key}, maybe the object did not report a change? It could NOT be fixed automatically, consider loading a backup.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking and fixing key {Key} in table {Table}: {ex.Message}");
        }
        finally
        {
            try
            {
                if (lockFirst) UnlockIgnore();
            }
            catch { }
        }
    }

    /// <summary>
    /// Sets the stored value to the given value and saves the state.
    /// </summary>
    public void SetValue(T value)
    {
        Value = value;
        Serialize();
    }

    /// <summary>
    /// Saves the current state of the value.
    /// </summary>
    public void Serialize()
    {
        Json = Serialization.Serialize<T>(Value);

        if (!Directory.Exists("../Database/Buffer/" + Table)) Directory.CreateDirectory("../Database/Buffer/" + Table);

        File.WriteAllBytes($"../Database/Buffer/{Table}/{Key}.json", Json);

        if (File.Exists($"../Database/{Table}/{Key}.json"))
            File.Delete($"../Database/{Table}/{Key}.json");

        File.Move($"../Database/Buffer/{Table}/{Key}.json", $"../Database/{Table}/{Key}.json");
    }

    /// <summary>
    /// Sets the value to a new object that is deserialized from the last saved state.
    /// </summary>
    private void Restore()
    {
        Value = Serialization.Deserialize<T>(Json);
        Value.ContainingEntry = this;
    }
}