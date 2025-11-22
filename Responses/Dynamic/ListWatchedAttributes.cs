using System.Collections;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class to contain multiple UI elements.
/// </summary>
public class ListWatchedAttributes(WatchedElement parent, List<(string Name, string? Value)> attributes) : IEnumerable<(string Name, string? Value)>
{
    /// <summary>
    /// The parent element.
    /// </summary>
    public readonly WatchedElement Parent = parent;
    
    /// <summary>
    /// The actual attributes.
    /// </summary>
    private readonly List<(string Name, string? Value)> _Attributes = attributes.ToList();
    
    /// <summary>
    /// Adds the given attribute to the end of the list.
    /// </summary>
    public void Add(string name, string? value)
    {
        _Attributes.Add((name, value));
        Parent.ChangeWatcher?.AttributeChanged(Parent, name);
    }
    
    /// <summary>
    /// Removes the given attribute from the list.
    /// </summary>
    public void Remove(string name, string? value)
    {
        _Attributes.Remove((name, value));
        Parent.ChangeWatcher?.AttributeChanged(Parent, name);
    }
    
    /// <summary>
    /// Removes all attributes from the list.
    /// </summary>
    public void Clear()
    {
        var names = _Attributes.Select(a => a.Name).Distinct().ToList();
        _Attributes.Clear();
        foreach (var name in names)
            Parent.ChangeWatcher?.AttributeChanged(Parent, name);
    }
    
    /// <summary>
    /// Replaces the current attributes in the list with the given attributes.
    /// </summary>
    public void ReplaceAll(List<(string Name, string? Value)> attributes)
    {
        HashSet<string> names = [];
        foreach (var attribute in _Attributes)
            names.Add(attribute.Name);
        foreach (var attribute in attributes)
            names.Add(attribute.Name);
        _Attributes.Clear();
        _Attributes.AddRange(attributes);
        foreach (var name in names)
            Parent.ChangeWatcher?.AttributeChanged(Parent, name);
    }

    public IEnumerator<(string Name, string? Value)> GetEnumerator()
    {
        foreach (var attribute in _Attributes)
            yield return attribute;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}