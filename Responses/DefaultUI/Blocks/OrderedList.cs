using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// An ordered list.
/// </summary>
public class OrderedList : AbstractList
{
    private RequiredWatchedAttribute TypeAttribute;
    
    public OrderedList(ListMarkerType type, IEnumerable<ListItem> items) : base(items)
    {
        TypeAttribute = new(this, "type", ConvertType(type));
    }
    
    public OrderedList(IEnumerable<ListItem> items) : this(ListMarkerType.Numbers, items)
    {
    }
    
    public string Type
    {
        get => TypeAttribute.Value;
        set => TypeAttribute.Value = value;
    }
    
    public override string RenderedTag
        => "ol";

    public override IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [
            ..base.WatchedAttributes,
            TypeAttribute
        ];

    public static string ConvertType(ListMarkerType type)
        => type switch
        {
            ListMarkerType.LettersUppercase => "A",
            ListMarkerType.LettersLowercase => "a",
            ListMarkerType.RomanNumbersUppercase => "I",
            ListMarkerType.RomanNumbersLowercase => "i",
            _ => "1"
        };
}