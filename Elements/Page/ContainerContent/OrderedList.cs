namespace uwap.WebFramework.Elements;

/// <summary>
/// Ordered list for a container.
/// </summary>
public class OrderedList : IContent
{
    //documentation inherited from IElement
    protected override string ElementType => "ol";

    //documentation inherited from IElement
    protected override string? ElementProperties
    {
        get
        {
            char t;
            switch (Type)
            {
                case Types.LettersUppercase:
                    t = 'A';
                    break;
                case Types.LettersLowercase:
                    t = 'a';
                    break;
                case Types.RomanNumbersUppercase:
                    t = 'I';
                    break;
                case Types.RomanNumbersLowercase:
                    t = 'i';
                    break;
                case Types.Numbers:
                default:
                    return null;
            }
            return $"type=\"{t}\"";
        }
    }

    /// <summary>
    /// The list of items.
    /// </summary>
    public List<string> List;

    /// <summary>
    /// The type of list item marker to use.
    /// </summary>
    public Types Type;

    /// <summary>
    /// Creates a new ordered list for a container with the given items.
    /// </summary>
    public OrderedList(List<string> list, Types type = Types.Numbers, string? classes = null, string? styles = null, string? id = null)
    {
        List = list;
        Type = type;
        Class = classes;
        Style = styles;
        Id = id;
    }

    /// <summary>
    /// Creates a new ordered list for a container with the given items.
    /// </summary>
    public OrderedList(params string[] items)
    {
        List = items.ToList();
        Class = null;
        Style = null;
        Id = null;
    }

    //documentation inherited from IContent
    public override IEnumerable<string> Export()
    {
        yield return Opener;

        foreach (string item in List)
            yield return $"\t<li>{item}</li>";

        yield return Closer;
    }

    public enum Types
    {
        Numbers,
        LettersUppercase,
        LettersLowercase,
        RomanNumbersUppercase,
        RomanNumbersLowercase
    }
}