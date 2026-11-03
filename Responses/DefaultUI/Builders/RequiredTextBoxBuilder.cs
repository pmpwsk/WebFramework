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
    Func<string, Task<string?>>? additionalValidator = null
) : IInputBuilder<C>
{
    public bool IgnoreForAutofocus
        => false;
    
    private TextBox? Element;
        
    public AbstractElement Initialize(C obj, Page page, bool autofocus)
    {
        var initialValue = reference(obj);
        Element = new TextBox(name, placeholder, initialValue, role, autofocus);
        return Element;
    }

    public async Task<string?> ValidateAsync()
    {
        if (Element == null)
            throw new Exception("Not initialized.");
        
        var value = Element.Value;
        
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