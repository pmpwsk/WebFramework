namespace uwap.WebFramework.Tools;

public delegate Task AsyncDelegateCaller<in T>(T subscriber);

public delegate Task<R> AsyncDelegateCallerWithResult<in T,R>(T subscriber);

public delegate void DelegateCaller<in T>(T subscriber);

public delegate R DelegateCallerWithResult<in T, out R>(T subscriber);

/// <summary>
/// Custom container for event subscribers of a certain type, mostly a callback function.
/// </summary>
public class SubscriberContainer<T> : IDisposable
{
    /// <summary>
    /// The lock to use when changing the list of subscribers.
    /// </summary>
    private AsyncLock Lock = new();
    
    /// <summary>
    /// The list of subscribed objects.
    /// </summary>
    private List<T> Subscribers = [];
    
    /// <summary>
    /// Adds the given subscriber.
    /// </summary>
    public async Task RegisterAsync(T subscriber)
    {
        using var h = await Lock.WaitAsync();
        Subscribers.Add(subscriber);
    }
    
    /// <summary>
    /// Removes the given subscriber.
    /// </summary>
    public async Task UnregisterAsync(T subscriber)
    {
        using var h = await Lock.WaitAsync();
        Subscribers.Remove(subscriber);
    }
    
    /// <summary>
    /// Removes all subscribers.
    /// </summary>
    public async Task ClearAsync()
    {
        using var h = await Lock.WaitAsync();
        Subscribers.Clear();
    }
    
    /// <summary>
    /// Calls all subscribers using the given caller function and handles any exceptions using the given exception handler.<br/>
    /// If <c>parallel</c> is set, the subscribers will be notified at the same time, otherwise they will be called in order.
    /// </summary>
    public async Task<bool> InvokeWithAsyncCaller(AsyncDelegateCaller<T> caller, Action<Exception>? exceptionHandler, bool parallel)
    {
        var subscribers = await ListAllAsync();
        if (subscribers.Count == 0)
            return false;
        
        if (parallel)
            await Task.WhenAll(subscribers.Select(subscriber => InvokeAsyncCaller(subscriber, caller, exceptionHandler)));
        else
            foreach (var subscriber in subscribers)
                await InvokeAsyncCaller(subscriber, caller, exceptionHandler);
        
        return true;
    }
    
    /// <summary>
    /// Calls the given subscriber using the given caller function and handles any exceptions using the given exception handler.
    /// </summary>
    private static Task InvokeAsyncCaller(T subscriber, AsyncDelegateCaller<T> caller, Action<Exception>? exceptionHandler)
    {
        try
        {
            return caller(subscriber);
        }
        catch (Exception ex)
        {
            if (exceptionHandler != null)
                exceptionHandler(ex);
            else throw;
            return Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Calls all subscribers using the given caller function and handles any exceptions using the given exception handler.
    /// </summary>
    public async Task<bool> InvokeWithSyncCaller(DelegateCaller<T> caller, Action<Exception>? exceptionHandler)
    {
        var subscribers = await ListAllAsync();
        if (subscribers.Count == 0)
            return false;
        
        foreach (var subscriber in subscribers)
            InvokeSyncCaller(subscriber, caller, exceptionHandler);
        
        return true;
    }
    
    /// <summary>
    /// Calls the given subscriber using the given caller function and handles any exceptions using the given exception handler.
    /// </summary>
    private static void InvokeSyncCaller(T subscriber, DelegateCaller<T> caller, Action<Exception>? exceptionHandler)
    {
        try
        {
            caller(subscriber);
        }
        catch (Exception ex)
        {
            if (exceptionHandler != null)
                exceptionHandler(ex);
            else throw;
        }
    }
    
    /// <summary>
    /// Calls all subscribers using the given caller function, collects the results and handles any exceptions using the given exception handler.
    /// </summary>
    public async Task<List<R>> InvokeWithAsyncCallerAndGet<R>(AsyncDelegateCallerWithResult<T,R> caller, Action<Exception>? exceptionHandler)
    {
        List<R> results = [];
        var subscribers = await ListAllAsync();
        foreach (var subscriber in subscribers)
            try
            {
                results.Add(await caller(subscriber));
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler(ex);
                else throw;
            }
        
        return results;
    }
    
    /// <summary>
    /// Calls all subscribers using the given caller function, collects the results and handles any exceptions using the given exception handler.
    /// </summary>
    public async Task<List<R>> InvokeWithSyncCallerAndGet <R>(DelegateCallerWithResult<T,R> caller, Action<Exception>? exceptionHandler)
    {
        List<R> results = [];
        var subscribers = await ListAllAsync();
        foreach (var subscriber in subscribers)
            try
            {
                results.Add(caller(subscriber));
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler(ex);
                else throw;
            }
        
        return results;
    }
    
    /// <summary>
    /// Lists all subscribers.
    /// </summary>
    private async Task<List<T>> ListAllAsync()
    {
        using var h = await Lock.WaitAsync();
        return Subscribers.ToList();
    }
    
    /// <summary>
    /// Whether the event has any subscribers.
    /// </summary>
    public async Task<bool> IsEmptyAsync()
    {
        using var h = await Lock.WaitAsync();
        return Subscribers.Count == 0;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Lock.Dispose();
    }
}