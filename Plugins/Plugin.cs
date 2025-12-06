using System.Collections.ObjectModel;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Abstract class to create plugins without implementing every single method.<br/>
/// The default implementation for handlers returns status 501 (not implemented), the worker method does nothing.
/// </summary>
public abstract class Plugin : IPlugin
{
    public virtual Task<IResponse> HandleAsync(Request req)
        => Task.FromResult<IResponse>(StatusResponse.NotImplemented);

    public virtual Task Work()
        => Task.CompletedTask;

    public virtual byte[]? GetFile(string relPath, string pathPrefix, string domain)
        => null;

    public virtual string? GetFileVersion(string relPath)
        => null;

    public virtual Task Backup(string id, ReadOnlyCollection<string> basedOnIds)
        => Task.CompletedTask;

    public virtual Task Restore(ReadOnlyCollection<string> ids)
        => Task.CompletedTask;
}
