using Microsoft.AspNetCore.Http;
using uwap.WebFramework;

namespace uwap.Database;

/// <summary>
/// Interface for table entries to allow unified locking/unlocking.
/// </summary>
internal interface ITableEntry
{
    /// <summary>
    /// Locks the entry.
    /// <returns>Whether the entry was locked using the current context or without a context at all.</returns>
    /// </summary>
    public bool Lock();

    /// <summary>
    /// Makes the entry's state persistent and unlocks it.
    /// </summary>
    public void UnlockSave();

    /// <summary>
    /// Restores the entry to the last persistent state and unlocks it.<br/>
    /// Note that the old value object should not be used anymore afterwards, since there will be a new object.
    /// </summary>
    public void UnlockRestore();

    /// <summary>
    /// Unlocks the entry while ignoring any changes to it (no saving or restoring).
    /// </summary>
    public void UnlockIgnore();
}