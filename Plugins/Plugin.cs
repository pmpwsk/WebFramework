using System.Collections.ObjectModel;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Abstract class to create plugins without implementing every single method.<br/>
/// The default implementation for handlers returns status 501 (not implemented), the worker method does nothing.
/// </summary>
public abstract class Plugin : IPlugin
{
    //documentation is inherited from IPlugin
    public virtual Task Handle(Request req)
        => throw new NotImplementedSignal();

    //documentation is inherited from IPlugin
    public virtual Task Work()
        => Task.CompletedTask;

    //documentation is inherited from IPlugin
    public virtual byte[]? GetFile(string relPath, string pathPrefix, string domain)
        => null;

    //documentation is inherited from IPlugin
    public virtual string? GetFileVersion(string relPath)
        => null;

    //documentation is inherited from IPlugin
    public virtual Task Backup(string id, ReadOnlyCollection<string> basedOnIds)
        => Task.CompletedTask;

    //documentation is inherited from IPlugin
    public virtual Task Restore(ReadOnlyCollection<string> ids)
        => Task.CompletedTask;
}
