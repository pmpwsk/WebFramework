using System.Text;

namespace uwap.WebFramework.Elements;

/// <summary>
/// Abstract class that elements can inherit to get a style, class, ID as well as methods for openers, closers and other combinations of those.
/// </summary>
public abstract class IElement
{
    /// <summary>
    /// The type of the element.
    /// </summary>
    protected abstract string ElementType {get;}

    /// <summary>
    /// A class that every element of this kind should have.
    /// </summary>
    protected abstract string? ElementClass {get;}

    /// <summary>
    /// Additional properties that every element of this kind should have.
    /// </summary>
    protected abstract string? ElementProperties {get;}

    /// <summary>
    /// The custom style of this element or null to disable.
    /// </summary>
    public string? Style;

    /// <summary>
    /// The custom class of this element or null to disable.
    /// </summary>
    public string? Class;

    /// <summary>
    /// The ID of this element or null to disable.
    /// </summary>
    public string? Id;

    /// <summary>
    /// Whether to allow HTML code in attributes of this element.<br/>
    /// Default: false
    /// </summary>
    public bool Unsafe = false;

    /// <summary>
    /// The opener of the element along with all of its properties, but without the last angle bracket.
    /// </summary>
    protected string OpenerWithoutEnd
    {
        get
        {
            StringBuilder result = new();
            result.Append($"<{ElementType}");

            if (ElementClass != null)
            {
                if (Class != null)
                    result.Append($" class=\"{ElementClass} {Class.HtmlValueSafe()}\"");
                else result.Append($" class=\"{ElementClass}\"");
            }
            else if (Class != null)
                result.Append($" class=\"{Class.HtmlValueSafe()}\"");

            if (ElementProperties != null)
                result.Append($" {ElementProperties}");

            if (Style != null)
                result.Append($" style=\"{Style.HtmlValueSafe()}\"");

            if (Id != null)
                result.Append($" id=\"{Id.HtmlValueSafe()}\"");
            
            return result.ToString();
        }
    }

    /// <summary>
    /// The opener of the element along with all of its properties.
    /// </summary>
    protected string Opener
        => OpenerWithoutEnd+">";
    
    /// <summary>
    /// The closer of the element.
    /// </summary>
    protected string Closer
        => $"</{ElementType}>";

    /// <summary>
    /// The element along with all of its properties but no content and no closer, it ends with a slash before the last angle bracket.
    /// </summary>
    protected string CodeWithoutExplicitCloser
        => OpenerWithoutEnd + " />";
}