namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A generic type for <c>ListWatchedContainer</c> to add typed elements to different levels.
/// </summary>
public interface IListWatchedContainer<in T> : IEnumerable<AbstractMarkdownPart?> where T : AbstractMarkdownPart?
{
    /// <summary>
    /// Adds the given element to the end of the list.
    /// </summary>
    public void Add(T element);
    
    /// <summary>
    /// Removes the given element from the list.
    /// </summary>
    public void Remove(T element);
    
    /// <summary>
    /// Removes all elements from the list.
    /// </summary>
    public void Clear();
}