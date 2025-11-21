using System.Collections.ObjectModel;

namespace uwap.WebFramework.Database;

/// <summary>
/// Keeps track of all tables that have been imported.
/// </summary>
public static class LegacyTables
{
    /// <summary>
    /// The dictionary of all imported tables (value) along with their names (key).
    /// </summary>
    public static Dictionary<string,ILegacyTable> Dictionary { get; set; } = [];

    /// <summary>
    /// All characters that are allowed as table names and keys.
    /// </summary>
    public static string KeyChars { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-+.=";

    /// <summary>
    /// Checks all tables for errors and attempts to fix them.<br/>
    /// If errors are found, more information is written to the console.
    /// </summary>
    public static void CheckAll()
    {
        foreach (var t in Dictionary)
            t.Value.CheckAndFix();
    }

    /// <summary>
    /// Backs up each table to a new part of the backup with the given ID. This is being called automatically.
    /// </summary>
    public static void BackupAll(string id, ReadOnlyCollection<string> basedOnIds)
    {
        foreach (var t in Dictionary)
            t.Value.Backup(id, basedOnIds);
    }

    /// <summary>
    /// Restores each table using the backups with the given IDs (ordered from the fresh backup to the last backup in the chain).
    /// </summary>
    public static void RestoreAll(ReadOnlyCollection<string> ids)
    {
        foreach (var t in Dictionary)
            t.Value.Restore(ids);
    }
}