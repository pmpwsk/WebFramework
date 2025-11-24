using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the file action list and commits the transaction when disposed. 
/// </summary>
public class ModifyTransactionData : IAsyncDisposable
{
    /// <summary>
    /// The file actions to execute.
    /// </summary>
    public List<IFileAction> FileActions = [];
    
    public readonly ReadyWaiter Waiter = new();
    
    /// <summary>
    /// Waits for the transaction to complete.
    /// </summary>
    public Task WaitAsync()
        => Waiter.WaitAsync(TimeSpan.FromSeconds(60));

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await Waiter.ReadyAsync();
    }
}