using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Builds a <c>Heading3</c> input label.
/// </summary>
public class InputLabelBuilder<C>(
    IconAndText text
) : IInputBuilder<C>
{
    public bool IgnoreForAutofocus
        => true;

    private Heading3? Element;
        
    public AbstractElement Initialize(C obj, Page page, bool autofocus)
    {
        Element = new(text);
        return Element;
    }

    public Task<string?> ValidateAsync()
        => Task.FromResult<string?>(null);
        
    public void Apply(C obj)
    {
    }
}