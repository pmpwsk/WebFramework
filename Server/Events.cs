using System.Collections.ObjectModel;

namespace uwap.WebFramework;

/// <summary>
/// Delegate for asynchronous handlers without arguments.
/// </summary>
public delegate Task AsyncAction();

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
    /// Called when the worker has finished working. This is meant so custom activities can be set to run after each worker tick.
    /// </summary>
    public static readonly SubscriberContainer<AsyncAction> WorkerWorked = new();

    /// <summary>
    /// Called when a request has been received.
    /// </summary>
    public static readonly SubscriberContainer<RequestHandler> RequestReceived = new();

    /// <summary>
    /// Called when the server is being backed up (after everything else has been backed up).
    /// </summary>
    public static readonly SubscriberContainer<BackupHandler> BackupAlmostDone = new();

    /// <summary>
    /// Called when the server is being restored from a backup (after everything else has been restored).
    /// </summary>
    public static readonly SubscriberContainer<RestoreHandler> RestoreAlmostDone = new();

    /// <summary>
    /// Called when the entire program has been requested to stop.
    /// </summary>
    public static readonly SubscriberContainer<Action> ProgramStopping = new();

    /// <summary>
    /// Called when the web server has successfully started and is ready to serve requests.
    /// </summary>
    public static readonly SubscriberContainer<Action> ServerReady = new();
}