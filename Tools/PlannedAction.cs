namespace uwap.WebFramework.Tools;

/// <summary>
/// Contains functionality to execute an action after a set amount of time.<br/>
/// Callers can reset the duration and cancel the action.  
/// </summary>
public class PlannedAction(TimeSpan waitDuration, Action action)
{
    /// <summary>
    /// The time after which the action should be executed.
    /// </summary>
    public readonly TimeSpan WaitDuration = waitDuration;
    
    /// <summary>
    /// The action to execute.
    /// </summary>
    public readonly Action Action = action;
    
    /// <summary>
    /// The cancellation token source to cancel the planned action. 
    /// </summary>
    private CancellationTokenSource? CancellationTokenSource = null;
    
    /// <summary>
    /// Starts waiting for the set duration and executes the action if the plans haven't been canceled. <br/>
    /// If the action is already planned, the duration will be reset. Exceptions within the action will be ignored.
    /// </summary>
    public void Start()
        => _ = StartAndWait();
    
    /// <summary>
    /// Starts waiting for the set duration and executes the action if the plans haven't been canceled.<br/>
    /// If the action is already planned, the duration will be reset. Exceptions within the action will be ignored.
    /// </summary>
    private async Task StartAndWait()
    {
        var cts = new CancellationTokenSource();
        DisposeSource(true, cts, null);

        try
        {
            await Task.Delay(WaitDuration, cts.Token);
        
            if (!cts.Token.IsCancellationRequested)
                Action();
        }
        catch
        {
        }
        
        DisposeSource(false, null, cts);
    }
    
    /// <summary>
    /// Cancels the planned execution of the action.
    /// </summary>
    public void Cancel()
        => DisposeSource(true, null, null);
    
    private void DisposeSource(bool cancel, CancellationTokenSource? replacement, CancellationTokenSource? caller)
    {
        var old = caller == null
            ? Interlocked.Exchange(ref CancellationTokenSource, replacement)
            : Interlocked.CompareExchange(ref CancellationTokenSource, replacement, caller);
        
        if (old == null)
            return;
        
        if (cancel)
            old.Cancel();
        
        old.Dispose();
    }
}