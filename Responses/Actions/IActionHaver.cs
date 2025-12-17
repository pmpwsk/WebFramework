namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// An interface for UI elements that accept an action call.
/// </summary>
public interface IActionHaver
{
    /// <summary>
    /// The action to perform when the form is submitted.
    /// </summary>
    public ActionHandler Action { get; }
}