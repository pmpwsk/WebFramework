using System.Diagnostics.CodeAnalysis;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.DefaultUI;

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

    private string? _SystemId = null;

    public List<AbstractWatchedContainer?> RenderedContainers { get; } = [];
    
    /// <summary>
    /// The attributes that can't change.
    /// </summary>
    public List<(string Name, string? Value)> FixedAttributes { get; } = [];
    
    /// <summary>
    /// The containers of attributes that can change.
    /// </summary>
    public List<AbstractWatchedAttribute> WatchedAttributes { get; } = [];

    /// <summary>
    /// The unique system ID within the parent.
    /// </summary>
    internal string? SystemId
    {
        get => FixedSystemId ?? _SystemId;
        set => _SystemId = value;
    }
    
    internal virtual string? FixedSystemId
        => null;
    
    public sealed override IEnumerable<AbstractMarkdownPart?> RenderedContent
        => RenderedContainers.WhereNotNull().SelectMany(container => container);
    
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
    
    /// <summary>
    /// Returns whether actions within this element should be ignored.
    /// </summary>
    public bool HasActionIgnoringParent
        => (this is AbstractOverlay overlay && overlay.IgnoreActions)
                || (ParentContainer != null && SystemId != null && ParentContainer.Parent is WatchedElement watchedParent && watchedParent.HasActionIgnoringParent);
    
    /// <summary>
    /// Attempts to retrieve the first child of the specified type with the specified condition.
    /// </summary>
    public bool TryGetFirstChild<T>(Predicate<T> condition, [MaybeNullWhen(false)] out T child) where T : AbstractMarkdownPart
    {
        foreach (var part in RenderedContent)
        {
            switch (part)
            {
                case T typedChild when condition(typedChild):
                    child = typedChild;
                    return true;
                case WatchedElement watchedChild when watchedChild.TryGetFirstChild(condition, out child):
                    return true;
            }
        }
        
        child = null;
        return false;
    }

    public override IEnumerable<(string Name, string? Value)> RenderedAttributes
        => [
            ("data-wf-id", SystemId),
            ..base.RenderedAttributes,
            ..FixedAttributes,
            ..WatchedAttributes.Select(a => a.Build())
        ];
}