namespace uwap.WebFramework.Responses.Base;

/// <summary>
/// An abstract HTML element.
/// </summary>
public abstract class AbstractElement : AbstractMarkdownPart
{
    /// <summary>
    /// The name of the HTML tag.
    /// </summary>
    public abstract string RenderedTag { get; }
    
    /// <summary>
    /// The attributes to render. These may contain multiple entries for the same name, the values of those will be merged with spaces in between.
    /// </summary>
    public virtual IEnumerable<(string Name, string? Value)> RenderedAttributes
        => [];
    
    /// <summary>
    /// The content/children to render.
    /// </summary>
    public virtual IEnumerable<AbstractMarkdownPart?> RenderedContent
        => [];
    
    /// <summary>
    /// Returns the combined value of the attribute with the given name, or null if it doesn't exist.
    /// </summary>
    public string? GetAttribute(string attributeName)
        => BuildAttributes().GetValueOrDefault(attributeName);
    
    /// <summary>
    /// Builds a dictionary of all attributes by combining their values.
    /// </summary>
    public Dictionary<string, string> BuildAttributes()
    {
        Dictionary<string, string> attributes = [];
        foreach (var attribute in RenderedAttributes)
            if (attribute.Value != null)
                attributes[attribute.Name] = attributes.TryGetValue(attribute.Name, out var oldValue) ? $"{oldValue} {attribute.Value}" : attribute.Value;
        return attributes;
    }
    
    /// <summary>
    /// Exports the element as raw text chunks.
    /// </summary>
    public sealed override IEnumerable<string> EnumerateChunks()
    {
        yield return "<";
        yield return RenderedTag;
        
        foreach (var attribute in BuildAttributes())
        {
            yield return " ";
            yield return attribute.Key;
            yield return "=\"";
            yield return attribute.Value.HtmlValueSafe();
            yield return "\"";
        }
        
        yield return ">";
        
        switch (RenderedTag)
        {
            case "area":
            case "base":
            case "br":
            case "col":
            case "command":
            case "embed":
            case "hr":
            case "img":
            case "input":
            case "keygen":
            case "link":
            case "meta":
            case "param":
            case "source":
            case "track":
            case "wbr":
                yield break;
        }
        
        foreach (var child in RenderedContent.WhereNotNull())
            foreach (var chunk in child.EnumerateChunks())
                yield return chunk;
        
        yield return "</";
        yield return RenderedTag;
        yield return ">";
    }
}