using uwap.WebFramework.Responses.Actions;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI selector input based on a dynamic dialog.<br/>
/// This element must be placed within a server form.
/// </summary>
public class DynamicSelector<T> : ButtonWithText, IActionHaver
{
    public ActionHandler Action
        => Open;
    
    public readonly Page Page;
    
    public readonly IconAndText Heading;
    
    public T Value;
    
    public List<DynamicSelectorItem<T>> Options;
    
    public DynamicSelector(Page page, IconAndText heading, T value, List<DynamicSelectorItem<T>> options)
        : base(GenerateContent(value, options))
    {
        Page = page;
        Heading = heading;
        Value = value;
        Options = options;
        FixedAttributes.Add(("class", "wf-button wf-server-form-override"));
        FixedAttributes.Add(("type", "submit"));
    }
    
    private Task<IActionResponse> Open(Request req)
        => DialogBuilder.DynamicDialogActionAsync(
            Page,
            Heading,
            [
                ..Options.Select(option => new BigServerSubmitButton(
                    option.Name,
                    option.Description == null ? [] : [ option.Description ],
                    _ =>
                    {
                        Value = option.Value;
                        Content = GenerateContent(option.Value, Options);
                        return DialogBuilder.DynamicDialogBackActionAsync(Page);
                    }
                )),
                new DialogBackButton(Page)
            ],
            Nothing.EmptyHandler
        );
    
    public override string RenderedTag
        => "button";
    
    private static IconAndText GenerateContent(T value, List<DynamicSelectorItem<T>> options)
    {
        foreach (var option in options)
            if (Matches(option.Value, value))
                return new IconAndText("bi bi-chevron-down", option.Name);
        
        throw new Exception("The given value isn't present in the options.");
    }
    
    private static bool Matches(T a, T b)
    {
        if (a == null)
            return b == null;
        else
            return a.Equals(b);
    }
}