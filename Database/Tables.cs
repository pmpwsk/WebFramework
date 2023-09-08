namespace uwap.Database;

/// <summary>
/// Keeps track of all tables that have been imported.
/// </summary>
public static class Tables
{
    /// <summary>
    /// The dictionary of all imported tables (value) along with their names (key).
    /// </summary>
    internal static Dictionary<string,ITable> Dictionary = new();

    /// <summary>
    /// All characters that are allowed as table names and keys.
    /// </summary>
    public static string KeyChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    /// Checks all tables for errors and attempts to fix them.<br/>
    /// If errors are found, more information is written to the console.
    /// </summary>
    public static void CheckAll()
    {
        foreach (var t in Dictionary)
            t.Value.CheckAndFix();
    }
}