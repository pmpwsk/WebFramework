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
    public virtual async Task Handle(AppRequest req, string rest, string pathPrefix)
    {
        Presets.CreatePage(req, "Untitled", out var page, out var elements);
        Presets.Navigation(req, page);
        int status = await HandleNeatly(req, rest, pathPrefix, page, elements);
        switch (status)
        {
            case 0:
            case 200:
                break;
            case -1:
                req.RedirectToLogin();
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(AppRequest req, string rest, string pathPrefix, Page page, List<IPageElement> elements)
     => Task.FromResult(501);

    //documentation is inherited from IPlugin
    public virtual async Task Handle(ApiRequest req, string rest, string pathPrefix)
    {
        int status = await HandleNeatly(req, rest, pathPrefix);
        switch (status)
        {
            case 0:
            case 200:
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(ApiRequest req, string rest, string pathPrefix)
        => Task.FromResult(501);

    //documentation is inherited from IPlugin
    public async virtual Task Handle(DownloadRequest req, string rest, string pathPrefix)
    {
        int status = await HandleNeatly(req, rest, pathPrefix);
        switch (status)
        {
            case 0:
            case 200:
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(DownloadRequest req, string rest, string pathPrefix)
        => Task.FromResult(501);

    //documentation is inherited from IPlugin
    public async virtual Task Handle(PostRequest req, string rest, string pathPrefix)
    {
        int status = await HandleNeatly(req, rest, pathPrefix);
        switch (status)
        {
            case 0:
            case 200:
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(PostRequest req, string rest, string pathPrefix)
        => Task.FromResult(501);

    //documentation is inherited from IPlugin
    public async virtual Task Handle(EventRequest req, string rest, string pathPrefix)
    {
        int status = await HandleNeatly(req, rest, pathPrefix);
        switch (status)
        {
            case 0:
            case 200:
                break;
            default:
                req.Status = status;
                break;
        }
    }
    protected virtual Task<int> HandleNeatly(EventRequest req, string rest, string pathPrefix)
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
