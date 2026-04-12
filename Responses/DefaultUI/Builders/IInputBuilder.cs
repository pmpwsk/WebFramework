using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Interface for classes to build elements that can be validated and applied against a specific type.
/// </summary>
public interface IInputBuilder<in C>
{
    public AbstractElement Initialize(C obj, Page page);
        
    public string? Validate();
        
    public void Apply(C obj);
}