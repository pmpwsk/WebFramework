using System.Globalization;
using uwap.WebFramework.Localization;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Builds a <c>TextBox</c> input based on a double property.
/// </summary>
public class DoubleTextBoxBuilder<C>(
    PropertyReference<C, double> reference,
    TextBoxRole role,
    string name,
    string placeholder,
    Func<double, Task<string?>>? additionalValidator = null
) : IInputBuilder<C>
{
    public bool IgnoreForAutofocus
        => false;
    
    private TextBox? Element;
        
    public AbstractElement Initialize(C obj, Page page, bool autofocus)
    {
        var initialValue = reference(obj).ToString(CultureInfo.InvariantCulture);
        Element = new TextBox(name, placeholder, initialValue, role, autofocus);
        return Element;
    }

    public async Task<string?> ValidateAsync()
    {
        if (Element == null)
            throw new Exception("Not initialized.");
        
        if (!double.TryParse(Element.Value, out var value))
            return CommonTexts.InvalidNumber;
        
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
            
        reference(obj) = double.Parse(Element.Value);
    }
}