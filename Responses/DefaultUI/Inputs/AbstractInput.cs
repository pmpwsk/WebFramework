using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An abstract default UI input.
/// </summary>
public abstract class AbstractInput : OptionalIdElement
{
    /// <summary>
    /// Sets the last known value, formatted as a string.
    /// </summary>
    public abstract void SetValueFromForm(string input);
    
    /// <summary>
    /// Whether this input field is required to submit the surrounding form.<br/>
    /// Default: true
    /// </summary>
    public bool Required { get; set; } = true;
    
    /// <summary>
    /// Whether the current value is unknown, empty, or only whitespace characters.
    /// </summary>
    public abstract bool IsEmpty { get; }
}