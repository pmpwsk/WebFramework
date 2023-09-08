using Microsoft.AspNetCore.Http;
using MimeKit.Encodings;
using System.Runtime.Serialization;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Abstract class for table values to allow unified locking/unlocking.
/// </summary>
[DataContract]
public abstract class ITableValue
{
    /// <summary>
    /// The table entry this value is contained in.
    /// </summary>
    internal ITableEntry? ContainingEntry = null;

    /// <summary>
    /// Locks this value's entry.
    /// <returns>Whether this value's entry was locked using the current context or without a context at all.</returns>
    /// </summary>
    public bool Lock()
    {
        if (ContainingEntry == null)
            throw new Exception("This object isn't part of a table.");
        return ContainingEntry.Lock();
    }

    /// <summary>
    /// Makes this value's entry's state persistent and unlocks it.
    /// </summary>
    public void UnlockSave()
    {
        if (ContainingEntry == null)
            throw new Exception("This object isn't part of a table.");
        ContainingEntry.UnlockSave();
    }

    /// <summary>
    /// Restores this value's entry to the last persistent state and unlocks it.
    /// </summary>
    public void UnlockRestore()
    {
        if (ContainingEntry == null)
            throw new Exception("This object isn't part of a table.");
        ContainingEntry.UnlockRestore();
    }

    /// <summary>
    /// Unlocks this value's entry while ignoring any changes to it (no saving or restoring).
    /// </summary>
    public void UnlockIgnore()
    {
        if (ContainingEntry == null)
            throw new Exception("This object isn't part of a table.");
        ContainingEntry.UnlockIgnore();
    }
}