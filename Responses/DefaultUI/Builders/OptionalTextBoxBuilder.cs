using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Builds a <c>TextBox</c> input based on a nullable string property.
/// </summary>
public class OptionalTextBoxBuilder<C>(
    PropertyReference<C, string?> reference,
    TextBoxRole role,
    string name,
    string placeholder,
    Func<string?, Task<string?>>? additionalValidator = null
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
        
        var value = Element.ValueNullable;
        
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
            
        reference(obj) = Element.IsEmpty(out var value) ? null : value;
    }
}