using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// An abstract element that is being watched for changes.
/// </summary>
public abstract class WatchedElement : AbstractElement, IWatchedParent
{
    /// <summary>
    /// The watched container holding this element.
    /// </summary>
    internal AbstractWatchedContainer? ParentContainer = null;
    
    /// <summary>
    /// The unique system ID within the parent.
    /// </summary>
    internal string? SystemId = null;
    
    public sealed override IEnumerable<AbstractMarkdownPart?> RenderedContent
        => RenderedContainers.WhereNotNull().SelectMany(container => container);

    public virtual IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [];
    
    public ChangeWatcher? ChangeWatcher
        => ParentContainer?.Parent.ChangeWatcher;
    
    public string[]? GetPath()
    {
        if (ParentContainer == null || SystemId == null)
            return null;
        if (ParentContainer.Parent is WatchedElement watchedParent)
        {
            var parentPath = watchedParent.GetPath();
            if (parentPath == null)
                return null;
            return [ ..parentPath, SystemId ];
        }
        else
            return [ SystemId ];
    }

    public override IEnumerable<(string Name, string? Value)> RenderedAttributes
        => [
            ("data-wf-id", SystemId),
            ..base.RenderedAttributes,
            ..FixedAttributes,
            ..WatchedAttributes.Select(a => a.Build())
        ];
    
    /// <summary>
    /// Enumerates the attributes that can't change.
    /// </summary>
    public virtual IEnumerable<(string Name, string? Value)> FixedAttributes
        => [];
    
    /// <summary>
    /// Enumerates the containers of attributes that can change.
    /// </summary>
    public virtual IEnumerable<AbstractWatchedAttribute> WatchedAttributes
        => [];
}