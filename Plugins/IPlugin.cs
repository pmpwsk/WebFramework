using System.Collections.ObjectModel;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Interface to create plugins for request handling and a worker method.<br/>
/// Initialization is not included as it should be done in the constructor.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Handles the given request.
    /// </summary>
    public Task<IResponse> HandleAsync(Request req);

    /// <summary>
    /// Does something that should be done regularly (every time the worker is active).
    /// </summary>
    public Task WorkAsync();

    /// <summary>
    /// Returns the bytes of a file to be served at the given relative path (and current plugin path prefix and current domain) or null if no such file exists.
    /// </summary>
    public byte[]? GetFile(string relPath, string pathPrefix, string domain);

    /// <summary>
    /// Returns the version/timestamp (for browser caching) of a file to be served at the given relative path or null if no such file exists.<br/>
    /// If a string (not null) is returned, the server will assume that GetFile(relPath) will not return null either.<br/>
    /// This should return a different value every time the file is changed (but never when it hasn't been modified) and shouldn't be long (DateTime.Ticks is the intended value).
    /// </summary>
    public string? GetFileVersion(string relPath);
}
