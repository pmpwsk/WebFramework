namespace uwap.WebFramework.Elements;

public abstract class IElement
{
    protected abstract string ElementType {get;}
    protected abstract string? ElementClass {get;}
    protected abstract string? ElementProperties {get;}

    public string? Style;
    public string? Class;
    public string? Id;

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

    protected string Opener => OpenerWithoutEnd+">";
        
    protected string Closer => $"</{ElementType}>";

    protected string CodeWithoutExplicitCloser
        => OpenerWithoutEnd + " />";
}