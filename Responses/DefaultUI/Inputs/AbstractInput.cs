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
}