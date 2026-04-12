using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Builds a <c>DynamicSelector</c> input based on a nullable string property.
/// </summary>
public class SelectorBuilder<C, T>(
    PropertyReference<C, T> reference,
    IconAndText heading,
    List<DynamicSelectorItem<T>> options
) : IInputBuilder<C>
{
    private DynamicSelector<T>? Element;
        
    public AbstractElement Initialize(C obj, Page page)
    {
        var initialValue = reference(obj);
        Element = new DynamicSelector<T>(page, heading, initialValue, options);
        return Element;
    }

    public string? Validate()
        => null;
        
    public void Apply(C obj)
    {
        if (Element == null)
            throw new Exception("Not initialized.");
            
        reference(obj) = Element.Value;
    }
}