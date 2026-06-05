using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Tools;

/// <summary>
/// Manages dependencies for asynchronous contexts.
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Holds the last dependency node for the current asynchronous context.
    /// </summary>
    private static AsyncLocal<DependencyNode?> Holder = new();
    
    /// <summary>
    /// Registers the dependency for the current asynchronous context.
    /// </summary>
    public static void Register(object dependency)
        => Holder.Value = new DependencyNode(Holder.Value, dependency);
    
    /// <summary>
    /// Registers the dependencies for the current asynchronous context.<br/>
    /// Registering multiple dependencies is more efficient using this method than individual calls.
    /// </summary>
    public static void RegisterRange(IEnumerable<object> dependencies)
    {
        var node = Holder.Value;
        bool dirty = false;
        
        foreach (var dependency in dependencies)
        {
            node = new(node, dependency);
            dirty = true;
        }
        
        if (dirty)
            Holder.Value = node;
    }
    
    /// <summary>
    /// Returns whether a dependency of this type is present in the asynchronous context and provides the last one.
    /// </summary>
    public static bool TryGet<T>([MaybeNullWhen(false)] out T dependency)
    {
        var node = Holder.Value;
        
        while (node != null)
        {
            if (node.Dependency is T value)
            {
                dependency = value;
                return true;
            }
            
            node = node.Previous;
        }
        
        dependency = default;
        return false;
    }
    
    /// <summary>
    /// Returns the last dependency of this type in the asynchronous context or <c>default</c>.
    /// </summary>
    public static T? GetNullable<T>()
        => TryGet(out T? dependency) ? dependency : default;
    
    /// <summary>
    /// Returns the last dependency of this type in the asynchronous context or throws and exception.
    /// </summary>
    public static T Get<T>()
        => TryGet(out T? dependency) && dependency != null ? dependency : throw new Exception($"Missing dependency '{typeof(T).Name}'.");
    
    /// <summary>
    /// Returns whether a dependency of this type is present in the asynchronous context and provides the first one.
    /// </summary>
    public static bool TryGetFirst<T>([MaybeNullWhen(false)] out T dependency)
    {
        var node = Holder.Value;
        dependency = default;
        bool found = false;
        
        while (node != null)
        {
            if (node.Dependency is T value)
            {
                dependency = value;
                found = true;
            }
            
            node = node.Previous;
        }
        
        return found;
    }
    
    /// <summary>
    /// Returns the first dependency of this type in the asynchronous context or <c>default</c>.
    /// </summary>
    public static T? GetFirstNullable<T>()
        => TryGetFirst(out T? dependency) ? dependency : default;
    
    /// <summary>
    /// Returns the first dependency of this type in the asynchronous context or throws and exception.
    /// </summary>
    public static T GetFirst<T>()
        => TryGetFirst(out T? dependency) && dependency != null ? dependency : throw new Exception($"Missing dependency '{typeof(T).Name}'.");
    
    /// <summary>
    /// A node in the chain of dependencies.
    /// </summary>
    private class DependencyNode(DependencyNode? previous, object dependency)
    {
        public readonly DependencyNode? Previous = previous;
        
        public readonly object Dependency = dependency;
    }
}