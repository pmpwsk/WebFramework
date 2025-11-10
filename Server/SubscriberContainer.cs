namespace uwap.WebFramework;

public delegate Task AsyncDelegateCaller<in T>(T subscriber);

public delegate Task<R> AsyncDelegateCallerWithResult<in T,R>(T subscriber);

public delegate void DelegateCaller<in T>(T subscriber);

public delegate R DelegateCallerWithResult<in T, out R>(T subscriber);

/// <summary>
/// Custom container for event subscribers of a certain type, mostly a callback function.
/// </summary>
public class SubscriberContainer<T>
{
    /// <summary>
    /// The lock to use when changing the list of subscribers.
    /// </summary>
    private ReaderWriterLockSlim Lock = new();
    
    /// <summary>
    /// The list of subscribed objects.
    /// </summary>
    private List<T> Subscribers = [];
    
    /// <summary>
    /// Adds the given subscriber.
    /// </summary>
    /// <param name="subscriber"></param>
    public void Register(T subscriber)
    {
        Lock.EnterWriteLock();
        Subscribers.Add(subscriber);
        Lock.ExitWriteLock();
    }
    
    /// <summary>
    /// Removes the given subscriber.
    /// </summary>
    /// <param name="subscriber"></param>
    public void Unregister(T subscriber)
    {
        Lock.EnterWriteLock();
        Subscribers.Add(subscriber);
        Lock.ExitWriteLock();
    }
    
    /// <summary>
    /// Removes all subscribers.
    /// </summary>
    public void Clear()
    {
        Lock.EnterWriteLock();
        Subscribers.Clear();
        Lock.ExitWriteLock();
    }
    
    /// <summary>
    /// Calls all subscribers using the given caller function and handles any exceptions using the given exception handler.<br/>
    /// If <c>parallel</c> is set, the subscribers will be notified at the same time, otherwise they will be called in order.
    /// </summary>
    public async Task<bool> InvokeAsync(AsyncDelegateCaller<T> caller, Action<Exception>? exceptionHandler, bool parallel)
    {
        var subscribers = ListAll();
        if (subscribers.Count == 0)
            return false;
        
        if (parallel)
            await Task.WhenAll(subscribers.Select(subscriber => InvokeAsync(subscriber, caller, exceptionHandler)));
        else
            foreach (var subscriber in subscribers)
                await InvokeAsync(subscriber, caller, exceptionHandler);
        
        return true;
    }
    
    /// <summary>
    /// Calls the given subscriber using the given caller function and handles any exceptions using the given exception handler.
    /// </summary>
    private static Task InvokeAsync(T subscriber, AsyncDelegateCaller<T> caller, Action<Exception>? exceptionHandler)
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
    public bool Invoke(DelegateCaller<T> caller, Action<Exception>? exceptionHandler)
    {
        var subscribers = ListAll();
        if (subscribers.Count == 0)
            return false;
        
        foreach (var subscriber in subscribers)
            Invoke(subscriber, caller, exceptionHandler);
        
        return true;
    }
    
    /// <summary>
    /// Calls the given subscriber using the given caller function and handles any exceptions using the given exception handler.
    /// </summary>
    private static void Invoke(T subscriber, DelegateCaller<T> caller, Action<Exception>? exceptionHandler)
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
    public async Task<List<R>> InvokeAndGetAsync<R>(AsyncDelegateCallerWithResult<T,R> caller, Action<Exception>? exceptionHandler)
    {
        List<R> results = [];
        var subscribers = ListAll();
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
    public List<R> InvokeAndGet<R>(DelegateCallerWithResult<T,R> caller, Action<Exception>? exceptionHandler)
    {
        List<R> results = [];
        var subscribers = ListAll();
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
    private List<T> ListAll()
    {
        Lock.EnterReadLock();
        var list = Subscribers.ToList();
        Lock.ExitReadLock();
        return list;
    }
    
    /// <summary>
    /// Whether the event has any subscribers.
    /// </summary>
    public bool IsEmpty()
        => ListAll().Count == 0;
}