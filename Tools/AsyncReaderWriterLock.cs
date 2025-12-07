namespace uwap.WebFramework.Tools;

/// <summary>
/// A simple lock that works asynchronously.
/// </summary>
public class AsyncReaderWriterLock : IDisposable
{
    private AsyncLock StateLock = new();
    
    private Queue<AsyncReaderWriterLockHolder> ReadWaiters = [];
    private Queue<AsyncReaderWriterLockHolder> WriteWaiters = [];
    
    private List<AsyncReaderWriterLockHolder> ReadHolders = [];
    private AsyncReaderWriterLockHolder? WriteHolder = null;
    
    private void UpdateHolders()
    {
        if (WriteHolder != null)
            return;
        
        var writeWaiting = WriteWaiters.Count != 0;
        var readWaiting = ReadWaiters.Count != 0;
        
        if (ReadHolders.Count == 0)
        {
            if (writeWaiting && readWaiting)
                if (IsWritePreferred())
                    StartWriter();
                else
                    StartReaders();
            else if (readWaiting)
                StartReaders();
            else if (writeWaiting)
                StartWriter();
        }
        else if (readWaiting && !IsWritePreferred())
            StartReaders();
    }
    
    private bool IsWritePreferred()
    {
        DateTime? writeTimestamp = WriteHolder?.Timestamp ?? WriteWaiters.PeekOrDefault()?.Timestamp;
        DateTime? readTimestamp = ReadHolders.FirstOrDefault()?.Timestamp ?? ReadWaiters.PeekOrDefault()?.Timestamp;
        
        if (writeTimestamp != null && readTimestamp != null)
            return writeTimestamp.Value < readTimestamp.Value;
        else
            return writeTimestamp != null;
    }
    
    private void StartReaders()
    {
        while (ReadWaiters.TryDequeue(out var reader))
        {
            ReadHolders.Add(reader);
            _ = reader.Waiter.ReadyAsync();
        }
    }
    
    private void StartWriter()
    {
        if (WriteWaiters.TryDequeue(out var writer))
        {
            WriteHolder = writer;
            _ = writer.Waiter.ReadyAsync();
        }
    }
    
    internal async Task HolderFinished(AsyncReaderWriterLockHolder holder)
    {
        using var h = await StateLock.WaitAsync(CancellationToken.None);
        
        if (holder.Writing)
        {
            if (WriteHolder != holder)
                return;
            
            WriteHolder = null;
        }
        else
        {
            if (!ReadHolders.Contains(holder))
                return;
            
            ReadHolders.Remove(holder);
        }
        
        UpdateHolders();
    }
    
    private async Task<AsyncReaderWriterLockHolder> WaitAsync(bool writing)
    {
        var holder = new AsyncReaderWriterLockHolder(this, writing);
        using (await StateLock.WaitAsync(CancellationToken.None))
        {
            if (writing)
                WriteWaiters.Enqueue(holder);
            else
                ReadWaiters.Enqueue(holder);
            
            UpdateHolders();
        }
        await holder.Waiter.WaitAsync();
        return holder;
    }
    
    /// <summary>
    /// Waits for the lock to be available and returns a new lock holder.<br/>
    /// The lock holder will release the lock when it is disposed, so it should be used in a <c>using</c>.
    /// </summary>
    public Task<AsyncReaderWriterLockHolder> WaitReadAsync()
        => WaitAsync(false);
    
    /// <summary>
    /// Waits for the lock to be available and returns a new lock holder.<br/>
    /// The lock holder will release the lock when it is disposed, so it should be used in a <c>using</c>.
    /// </summary>
    public Task<AsyncReaderWriterLockHolder> WaitWriteAsync()
        => WaitAsync(true);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        StateLock.Dispose();
        foreach (var holder in ReadWaiters)
            holder.Dispose();
        foreach (var holder in WriteWaiters)
            holder.Dispose();
        foreach (var holder in ReadHolders)
            holder.Dispose();
        WriteHolder?.Dispose();
    }
}

/// <summary>
/// An object holding a lock for the <c>AsyncReaderWriterLock</c> class.
/// </summary>
public class AsyncReaderWriterLockHolder : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The time the lock request was started.
    /// </summary>
    internal readonly DateTime Timestamp;
    
    /// <summary>
    /// The waiter indicating when the lock can be entered.
    /// </summary>
    internal readonly ReadyWaiter Waiter;
    
    /// <summary>
    /// Whether the holder is for a writing lock.
    /// </summary>
    internal readonly bool Writing;
    
    /// <summary>
    /// The lock being held.
    /// </summary>
    public readonly AsyncReaderWriterLock Parent;
        
    internal AsyncReaderWriterLockHolder(AsyncReaderWriterLock parent, bool writing)
    {
        Timestamp = DateTime.UtcNow;
        Waiter = new();
        Writing = writing;
        Parent = parent;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Parent.HolderFinished(this);
        Waiter.Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Waiter.Dispose();
    }
}