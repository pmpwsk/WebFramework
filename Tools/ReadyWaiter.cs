namespace uwap.WebFramework.Tools;

/// <summary>
/// A waiter that waits until <c>Ready()</c> is called.
/// </summary>
public class ReadyWaiter : IDisposable
{
    private AsyncLock StateLock = new();
    
    internal SemaphoreSlim Semaphore = new(0, 1);
    
    /// <summary>
    /// Whether the call is ready.
    /// </summary>
    public bool IsReady { get; private set; } = false;
    
    private bool EverWaited = false;
    
    /// <summary>
    /// Waits for the call to be ready.
    /// </summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using (await StateLock.WaitAsync(cancellationToken))
        {
            if (IsReady)
                return;
            
            if (EverWaited)
                throw new Exception("Only one thread can wait for the call to be ready.");
            
            EverWaited = true;
        }
        
        await Semaphore.WaitAsync(timeout, cancellationToken);
    }
    
    /// <summary>
    /// Waits for the call to be ready.
    /// </summary>
    public Task WaitAsync(TimeSpan timeout)
        => WaitAsync(timeout, CancellationToken.None);
    
    /// <summary>
    /// Waits for the call to be ready.
    /// </summary>
    public Task WaitAsync(CancellationToken cancellationToken)
        => WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);
    
    /// <summary>
    /// Waits for the call to be ready.
    /// </summary>
    public Task WaitAsync()
        => WaitAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);
    
    /// <summary>
    /// Sets the call as "ready", releasing the waiter.<br/>
    /// Returns <c>true</c> if the state was changed and <c>false</c> if the call was already ready.
    /// </summary>
    public async Task<bool> ReadyAsync()
    {
        using var h = await StateLock.WaitAsync();
        
        if (IsReady)
            return false;
    
        IsReady = true;
        Semaphore.Release();
        return true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        StateLock.Dispose();
        Semaphore.Dispose();
    }
}