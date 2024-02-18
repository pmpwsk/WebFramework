using System.Collections.ObjectModel;

namespace uwap.WebFramework;

/// <summary>
/// Delegate for asynchronous handlers without arguments.
/// </summary>
public delegate Task Handler();

/// <summary>
/// Delegate for asynchronous API request handlers.
/// </summary>
public delegate Task ApiRequestHandler(ApiRequest request);

/// <summary>
/// Delegate for asynchronous app request handlers.
/// </summary>
public delegate Task AppRequestHandler(AppRequest request);

/// <summary>
/// Delegate for asynchronous download request handlers.
/// </summary>
public delegate Task DownloadRequestHandler(DownloadRequest request);

/// <summary>
/// Delegate for asynchronous upload request handlers.
/// </summary>
public delegate Task UploadRequestHandler(UploadRequest request);

/// <summary>
/// Delegate for asynchronous POST request handlers.
/// </summary>
public delegate Task PostRequestHandler(PostRequest request);

/// <summary>
/// Delegate for asynchronous event request handlers.
/// </summary>
public delegate Task EventRequestHandler(EventRequest request);

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
    /// Called when an API request has been received.
    /// </summary>
    public static event ApiRequestHandler? ApiRequestReceived = null;

    /// <summary>
    /// Called when an app request has been received.
    /// </summary>
    public static event AppRequestHandler? AppRequestReceived = null;

    /// <summary>
    /// Called when a download request has been received.
    /// </summary>
    public static event DownloadRequestHandler? DownloadRequestReceived = null;

    /// <summary>
    /// Called when an upload request has been received.
    /// </summary>
    public static event UploadRequestHandler? UploadRequestReceived = null;

    /// <summary>
    /// Called when a POST request without files has been received.
    /// </summary>
    public static event PostRequestHandler? PostRequestReceived = null;

    /// <summary>
    /// Called when an event request has been received.
    /// </summary>
    public static event EventRequestHandler? EventRequestReceived = null;

    /// <summary>
    /// Called when the server is being backed up (after everything else has been backed up).
    /// </summary>
    public static event BackupHandler? BackupAlmostDone = null;
}