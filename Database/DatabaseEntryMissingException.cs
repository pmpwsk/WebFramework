namespace uwap.WebFramework.Database;

/// <summary>
/// An exception indicating that the database entry has not been found. 
/// </summary>
public class DatabaseEntryMissingException() : Exception("The database entry is missing.");