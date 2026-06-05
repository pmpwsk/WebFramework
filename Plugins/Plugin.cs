using uwap.WebFramework.Responses;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Abstract class to create plugins without implementing every single method.<br/>
/// The default implementation for handlers returns status 501 (not implemented), the worker method does nothing.
/// </summary>
public abstract class Plugin : IPlugin
{
    protected List<object> PluginDependencies = [];

    public IEnumerable<object> EnumerateDependencies()
        => PluginDependencies;
    
    public virtual Task<IResponse> HandleAsync(Request req)
        => Task.FromResult<IResponse>(StatusResponse.NotImplemented);

    public virtual Task WorkAsync()
        => Task.CompletedTask;

    public virtual byte[]? GetFile(string relPath, string pathPrefix, string domain)
        => null;

    public virtual string? GetFileVersion(string relPath)
        => null;
}
