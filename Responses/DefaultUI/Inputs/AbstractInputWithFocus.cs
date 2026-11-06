namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI input that can be focused.
/// </summary>
public abstract class AbstractInputWithFocus : AbstractInput
{
    /// <summary>
    /// Whether the field should be in focus when the page is loaded.
    /// </summary>
    public abstract bool Autofocus { get; set; }
    
    /// <summary>
    /// Requests that the browser focuses this field.
    /// </summary>
    public abstract void Focus();
}