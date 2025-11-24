using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Describes a lock request in a WebFramework database cluster.
/// </summary>
public class LockRequest : FunctionalComparable<LockRequest>
{
    /// <summary>
    /// Creates, queues and distributes a new lock request for the given entry that will be executed by this program instance.
    /// </summary>
    public static Task<LockRequest> CreateLocalAsync(AbstractTableEntry entry)
        => CreateAsync(entry, DateTime.UtcNow.Ticks, Parsers.RandomString(32), true);
    
    /// <summary>
    /// Adds another program instance's lock request with the given metadata to the given entry's lock queue.
    /// </summary>
    public static Task<LockRequest> CreateRemoteAsync(AbstractTableEntry entry, long timestamp, string randomness)
        => CreateAsync(entry, timestamp, randomness, false);
    
    /// <summary>
    /// Creates a new lock request object with the given metadata and distributes the request to other nodes if <c>distribute</c> is set.
    /// </summary>
    private static async Task<LockRequest> CreateAsync(AbstractTableEntry entry, long timestamp, string randomness, bool distribute)
    {
        HashSet<(long Timestamp, string Randomness)> others = [];
        
        LockRequest? request;
        using (await entry.LockRequestStateLock.WaitAsync())
        {
            request = entry.LockRequests.FirstOrDefault(lockReq => lockReq.Timestamp == timestamp && lockReq.Randomness == randomness);
            if (request == null)
            {
                request = new(entry, timestamp, randomness);
                entry.LockRequests.Add(request);
                
                if (distribute && Tables.Dictionary.TryGetValue(entry.Table.Name, out var table))
                {
                    var reachable = table.GetReachableNodes();
                    var results = Task.WhenAll(reachable.Select(node => node.SendLockAsync(table, entry.Id, timestamp, randomness))).GetAwaiter().GetResult();
                    foreach (var result in results.OfType<string>())
                        foreach (var pair in result.Split('&'))
                            if (!entry.DeletedLockRequests.Contains(pair) && pair.SplitAtFirst(';', out var otherTimestampString, out var otherRandomness) && long.TryParse(otherTimestampString, out var otherTimestamp))
                                others.Add((otherTimestamp, otherRandomness));
                }
            }
            
            var first = entry.LockRequests.First();
            await first.SetReadyAsync();
        }
        
        foreach (var other in others)
            await CreateRemoteAsync(entry, other.Timestamp, other.Randomness);
        
        return request;
    }
    
    /// <summary>
    /// Removes the given lock request from the entry's queue without distributing the information to other nodes.
    /// </summary>
    public static async Task DeleteAsync(AbstractTableEntry entry, long timestamp, string randomness)
    {
        if (timestamp == 0 && randomness == "none")
            return;
        
        LockRequest? request;
        using (await entry.LockRequestStateLock.WaitAsync())
        {
            entry.DeletedLockRequests.Add($"{timestamp};{randomness}");
            request = entry.LockRequests.FirstOrDefault(lockReq => lockReq.Timestamp == timestamp && lockReq.Randomness == randomness);
        }
        if (request != null)
            await request.SetFinishedAsync();
    }
    
    /// <summary>
    /// The table entry the lock request belongs to.
    /// </summary>
    public readonly AbstractTableEntry Entry;
    
    /// <summary>
    /// The timestamp ticks indicating when the lock request was started.
    /// </summary>
    public readonly long Timestamp;
    
    /// <summary>
    /// Randomness to distinguish between lock requests with the same timestamp.
    /// </summary>
    public readonly string Randomness;
    
    /// <summary>
    /// The lock to use when changing the state of the lock request.
    /// </summary>
    private readonly AsyncLock Lock = new();
    
    /// <summary>
    /// The waiter that can be awaited to wait for readiness.
    /// </summary>
    private readonly ReadyWaiter Waiter = new();
    
    /// <summary>
    /// Whether the transaction has been finished yet.
    /// </summary>
    public bool Finished { get; private set; } = false;
    
    /// <summary>
    /// Creates a new lock request object with the given data.
    /// </summary>
    private LockRequest(AbstractTableEntry entry, long timestamp, string randomness)
    {
        Entry = entry;
        Timestamp = timestamp;
        Randomness = randomness;
    }
    
    /// <summary>
    /// Waits until the transaction is allowed to run.
    /// </summary>
    public Task WaitUntilReady()
        => Waiter.WaitAsync(Server.Config.Database.LockExpiration * 10);
    
    /// <summary>
    /// Allows the transaction to start.
    /// </summary>
    public async Task SetReadyAsync()
    {
        using var h = await Lock.WaitAsync();
        
        if (await Waiter.ReadyAsync())
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(Server.Config.Database.LockExpiration);
                await SetFinishedAsync();
            });
        }
    }
    
    /// <summary>
    /// Indicates that the transaction has been completed.
    /// </summary>
    public async Task SetFinishedAsync()
    {
        using var h = await Lock.WaitAsync();
        
        if (!Finished)
        {
            Finished = true;
            
            using var h2 = await Entry.LockRequestStateLock.WaitAsync();
            
            Entry.LockRequests.Remove(this);
            
            var first = Entry.LockRequests.FirstOrDefault();
            if (first != null)
                await first.SetReadyAsync();
        }
    }

    protected override IEnumerable<Func<LockRequest, IComparable>> EnumerateComparators()
        => [ x => x.Timestamp, x => x.Randomness ];
}