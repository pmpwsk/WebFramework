using System.Collections.ObjectModel;

namespace uwap.WebFramework;

/// <summary>
/// Delegate for asynchronous handlers without arguments.
/// </summary>
public delegate Task Handler();

/// <summary>
/// Delegate for asynchronous request handlers.
/// </summary>
public delegate Task RequestHandler(Request req);

/// <summary>
/// Delegate for backup handlers.
/// </summary>
public delegate Task BackupHandler(string id, ReadOnlyCollection<string> basedOnIds);

/// <summary>
/// Delegate for restore handlers.
/// </summary>
public delegate Task RestoreHandler(ReadOnlyCollection<string> ids);

public static partial class Server
{
    /// <summary>
    /// Called when the worker has finished working. This is meant so custom activities can be set to run afterwards.
    /// </summary>
    public static event Handler? WorkerWorked = null;

    /// <summary>
    /// Called when a request has been received.
    /// </summary>
    public static event RequestHandler? RequestReceived = null;

    /// <summary>
    /// Called when the server is being backed up (after everything else has been backed up).
    /// </summary>
    public static event BackupHandler? BackupAlmostDone = null;

    /// <summary>
    /// Called when the server is being restored from a backup (after everything else has been restored).
    /// </summary>
    public static event RestoreHandler? RestoreAlmostDone = null;
}