using System.Collections.ObjectModel;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Abstract class to create plugins without implementing every single method.<br/>
/// The default implementation for handlers returns status 501 (not implemented), the worker method does nothing.
/// </summary>
public abstract class Plugin : IPlugin
{
    //documentation is inherited from IPlugin
    public virtual async Task Handle(Request req)
    {
        int status = await HandleNeatly(req);
        switch (status)
        {
            case 0:
                break;
            case -1:
                req.RedirectToLogin();
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(Request req)
        => Task.FromResult(501);

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
