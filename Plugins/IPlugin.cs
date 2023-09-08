using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Interface to create plugins for request handling and a worker method.<br/>
/// Initialization is not included as it should be done in the constructor.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Handles the given page request with the given relative path and plugin path (without the domain or preceding slash).
    /// </summary>
    public Task Handle(AppRequest req, string relPath, string pathPrefix);

    /// <summary>
    /// Handles the given API request with the given relative path and plugin path (without the domain or preceding slash).
    /// </summary>
    public Task Handle(ApiRequest req, string relPath, string pathPrefix);

    /// <summary>
    /// Handles the given upload request with the given relative path and plugin path (without the domain or preceding slash).
    /// </summary>
    public Task Handle(UploadRequest req, string relPath, string pathPrefix);

    /// <summary>
    /// Handles the given download request with the given relative path and plugin path (without the domain or preceding slash).
    /// </summary>
    public Task Handle(DownloadRequest req, string relPath, string pathPrefix);

    /// <summary>
    /// Handles the given POST request (without any files) with the given relative path and plugin path (without the domain or preceding slash).
    /// </summary>
    public Task Handle(PostRequest req, string relPath, string pathPrefix);

    /// <summary>
    /// Does something that should be done regularly (every time the worker is active).
    /// </summary>
    public Task Work();

    /// <summary>
    /// Returns the bytes of a file to be served at the given relative path or null if no such file exists.
    /// </summary>
    public byte[]? GetFile(string relPath, string pathPrefix);

    /// <summary>
    /// Returns the version/timestamp (for browser caching) of a file to be served at the given relative path or null if no such file exists.<br/>
    /// If a string (not null) is returned, the server will assume that GetFile(relPath) will not return null either.<br/>
    /// This should return a different value every time the file is changed (but never when it hasn't been modified) and shouldn't be long (DateTime.Ticks is the intended value).
    /// </summary>
    public string? GetFileVersion(string relPath);
}
