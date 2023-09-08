using System;
namespace uwap.WebFramework.Elements;

public class Selector : IContent
{
    protected override string ElementType => "select";

    public List<SelectorItem> Items;

    public Selector(string id, params string[] items)
    {
        Id = id;
        Items = items.Select(x => new SelectorItem(x)).ToList();
    }

    public Selector(string id, IEnumerable<string> items)
    {
        Id = id;
        Items = items.Select(x => new SelectorItem(x)).ToList();
    }

    public Selector(string id, params SelectorItem[] items)
    {
        Id = id;
        Items = items.ToList();
    }

    public Selector(string id, IEnumerable<SelectorItem> items)
    {
        Id = id;
        Items = items.ToList();
    }

    public override ICollection<string> Export()
    {
        List<string> result = new();
        result.Add(Opener);
        foreach (var item in Items)
            result.Add("\t" + item.Export());
        result.Add(Closer);
        return result;
    }
}

public struct SelectorItem
{
    public string Text, Value;

    public SelectorItem(string text, string value)
    {
        Text = text;
        Value = value;
    }

    public SelectorItem(string textAndValue)
    {
        Text = textAndValue;
        Value = textAndValue;
    }

    public string Export()
        => $"<option value=\"{Value}\">{Text}</option>";
}