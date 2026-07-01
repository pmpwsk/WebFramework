using System.Reflection;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Abstract class to create plugins without implementing every single method.<br/>
/// The default implementation for handlers returns status 501 (not implemented), the worker method does nothing.
/// </summary>
public abstract class Plugin : IPlugin
{
    protected List<object> PluginDependencies = [];
    
    private Dictionary<string, Func<Request, Task<IResponse>>> PluginEndpoints;
    
    protected Plugin()
    {
        PluginEndpoints = Endpoints.BuildEndpoints(this);
    }

    public IEnumerable<object> EnumerateDependencies()
        => PluginDependencies;
    
    public Task<IResponse> HandleAsync(Request req)
        => PluginEndpoints.TryGetValue(req.Path, out var endpoint)
            ? endpoint.Invoke(req)
            : HandleOtherAsync(req);
    
    public virtual Task<IResponse> HandleOtherAsync(Request req)
        => Task.FromResult<IResponse>(StatusResponse.NotFound);

    public virtual Task WorkAsync()
        => Task.CompletedTask;

    public virtual byte[]? GetFile(string relPath, string pathPrefix, string domain)
        => null;

    public virtual string? GetFileVersion(string relPath)
        => null;
}
