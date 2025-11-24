namespace uwap.WebFramework.Database;

/// <summary>
/// Contains the table value and file action list 
/// </summary>
public class TransactionData<T>(T value, List<IFileAction>? fileActions = null)
{
    /// <summary>
    /// The transaction's current value.
    /// </summary>
    public T Value = value;
    
    /// <summary>
    /// The file actions to execute.
    /// </summary>
    public List<IFileAction> FileActions = fileActions ?? [];
}