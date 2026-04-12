using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Interface for classes to build elements that can be validated and applied against a specific type.
/// </summary>
public interface IInputBuilder<in C>
{
    /// <summary>
    /// Creates and stores the input element to use.
    /// </summary>
    public AbstractElement Initialize(C obj, Page page);
    
    /// <summary>
    /// Validates the current value of the input element and returns an error message if necessary.
    /// </summary>
    /// <returns></returns>
    public Task<string?> ValidateAsync();
    
    /// <summary>
    /// Applies the current value of the input element to the object.
    /// </summary>
    public void Apply(C obj);
}