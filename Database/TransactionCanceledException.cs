namespace uwap.WebFramework.Database;

/// <summary>
/// An exception indicating that the modification transaction has been canceled.
/// </summary>
public class TransactionCanceledException() : Exception("The transaction was canceled.");