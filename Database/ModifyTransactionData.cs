using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the file action list and commits the transaction when disposed. 
/// </summary>
public class ModifyTransactionData : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The file actions to execute.
    /// </summary>
    public List<IFileAction> FileActions = [];
    
    /// <summary>
    /// The waiter used to pause the transaction.
    /// </summary>
    public readonly ReadyWaiter Waiter = new();
    
    /// <summary>
    /// Whether the transaction should be canceled.
    /// </summary>
    public bool Cancelled { get; private set; } = false;
    
    /// <summary>
    /// The transaction task.
    /// </summary>
    internal Task? Task = null;
    
    /// <summary>
    /// Waits for the transaction to complete.
    /// </summary>
    public Task WaitAsync()
        => Waiter.WaitAsync(TimeSpan.FromSeconds(60));
    
    /// <summary>
    /// Sets the transaction to be canceled upon disposal, without reverting the value.
    /// </summary>
    public void RequestCancellation()
        => Cancelled = true;

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Waiter.ReadyAsync();
        if (Task != null)
            try
            {
                await Task;
            }
            catch (TransactionCanceledException) { }
        Waiter.Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Waiter.Dispose();
    }
}