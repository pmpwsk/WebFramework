namespace uwap.WebFramework.Tools;

/// <summary>
/// A simple lock that works asynchronously.
/// </summary>
public class AsyncLock
{
    internal SemaphoreSlim Semaphore = new(1, 1);
    
    /// <summary>
    /// Waits for the lock to be available and returns a new lock holder.<br/>
    /// The lock holder will release the lock when it is disposed, so it should be used in a <c>using</c>.
    /// </summary>
    public async Task<AsyncLockHolder> WaitAsync(CancellationToken cancellationToken)
    {
        await Semaphore.WaitAsync(cancellationToken);
        return new(this);
    }
    
    /// <summary>
    /// Waits for the lock to be available and returns a new lock holder.<br/>
    /// The lock holder will release the lock when it is disposed, so it should be used in a <c>using</c>.
    /// </summary>
    public Task<AsyncLockHolder> WaitAsync()
        => WaitAsync(CancellationToken.None);
}

/// <summary>
/// An object holding a lock for the <c>AsyncLock</c> class.
/// </summary>
public class AsyncLockHolder : IDisposable
{
    /// <summary>
    /// The lock being held.
    /// </summary>
    public readonly AsyncLock Parent;
        
    internal AsyncLockHolder(AsyncLock parent)
    {
        Parent = parent;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Parent.Semaphore.Release();
    }
}