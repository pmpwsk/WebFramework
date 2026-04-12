using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Builds a <c>TextBox</c> input based on a non-nullable string property.
/// </summary>
public class RequiredTextBoxBuilder<C>(
    PropertyReference<C, string> reference,
    TextBoxRole role,
    string name,
    string placeholder,
    string missingMessage,
    Func<string, Task<string?>>? additionalValidator = null
) : IInputBuilder<C>
{
    private TextBox? Element;
        
    public AbstractElement Initialize(C obj, Page page)
    {
        var initialValue = reference(obj);
        Element = new TextBox(name, placeholder, initialValue, role);
        return Element;
    }

    public async Task<string?> ValidateAsync()
    {
        if (Element == null)
            throw new Exception("Not initialized.");
            
        if (Element.IsEmpty(out var value))
            return missingMessage;
        
        if (additionalValidator != null)
        {
            var message = await additionalValidator(value);
            if (message != null)
                return message;
        }
            
        return null;
    }
        
    public void Apply(C obj)
    {
        if (Element == null)
            throw new Exception("Not initialized.");
            
        reference(obj) = Element.Value;
    }
}