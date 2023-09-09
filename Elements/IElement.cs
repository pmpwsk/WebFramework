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
    /// The opener of the element along with all of its properties, but without the last angle bracket.
    /// </summary>
    protected string OpenerWithoutEnd
    {
        get
        {
            string o = $"<{ElementType}";

            if (ElementClass != null)
            {
                if (Class != null) o += $" class=\"{ElementClass} {Class}\"";
                else o += $" class=\"{ElementClass}\"";
            }
            else if (Class != null) o += $" class=\"{Class}\"";

            if (ElementProperties != null)
                o += $" {ElementProperties}";

            if (Style != null)
                o += $" style=\"{Style}\"";

            if (Id != null)
                o += $" id=\"{Id}\"";
            
            //o += ">";
            return o;
        }
    }

    /// <summary>
    /// The opener of the element along with all of its properties.
    /// </summary>
    protected string Opener => OpenerWithoutEnd+">";
    
    /// <summary>
    /// The closer of the element.
    /// </summary>
    protected string Closer => $"</{ElementType}>";

    /// <summary>
    /// The element along with all of its properties but no content and no closer, it ends with a slash before the last angle bracket.
    /// </summary>
    protected string CodeWithoutExplicitCloser
        => OpenerWithoutEnd + " />";
}