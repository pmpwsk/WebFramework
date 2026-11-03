namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// An interface for UI elements that accept an action call.
/// </summary>
public interface IActionHaver
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public ActionHandlerAsync Action { get; }
    
    /// <summary>
    /// Whether this action should be performed regardless of whether any required fields are empty.
    /// </summary>
    public bool IgnoresRequired { get; }
}