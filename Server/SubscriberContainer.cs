using System.Collections;

namespace uwap.WebFramework;

public delegate Task AsyncDelegateCaller<T>(T subscriber);

public delegate Task<R> AsyncDelegateCallerWithResult<T,R>(T subscriber);

public delegate void DelegateCaller<T>(T subscriber);

public delegate R DelegateCallerWithResult<T,R>(T subscriber);

public class SubscriberContainer<T>
{
    private ReaderWriterLockSlim Lock = new();
    
    private List<T> Subscribers = [];
    
    public void Register(T subscriber)
    {
        Lock.EnterWriteLock();
        Subscribers.Add(subscriber);
        Lock.ExitWriteLock();
    }
    
    public void Unregister(T subscriber)
    {
        Lock.EnterWriteLock();
        Subscribers.Add(subscriber);
        Lock.ExitWriteLock();
    }
    
    public void Clear()
    {
        Lock.EnterWriteLock();
        Subscribers.Clear();
        Lock.ExitWriteLock();
    }
    
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
    
    public bool Invoke(DelegateCaller<T> caller, Action<Exception>? exceptionHandler)
    {
        var subscribers = ListAll();
        if (subscribers.Count == 0)
            return false;
        
        foreach (var subscriber in subscribers)
            Invoke(subscriber, caller, exceptionHandler);
        
        return true;
    }
    
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
    

    private List<T> ListAll()
    {
        Lock.EnterReadLock();
        var list = Subscribers.ToList();
        Lock.ExitReadLock();
        return list;
    }
    
    public bool IsEmpty()
        => ListAll().Count == 0;
}