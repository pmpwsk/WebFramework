using System.Runtime.CompilerServices;

namespace uwap.WebFramework.Tools;

/// <summary>
/// A class that allows for simple comparator implementations by enumerating which fields to compare (in order).<br/>
/// The compared type must be the type of the final class, like <c>class Test : FunctionalComparable&lt;Test&gt;</c>
/// </summary>
public abstract class FunctionalComparable<T> : IComparable<T> where T : FunctionalComparable<T>
{
    protected FunctionalComparable()
    {
        if (this is not T)
            throw new Exception("FunctionalComparable requires both sides to have the same type.");
    }
 
    /// <summary>
    /// Enumerates the fields to compare by specifying selectors (in order).
    /// </summary>
    protected abstract IEnumerable<Func<T, IComparable>> EnumerateComparators();

    public int CompareTo(T? other)
    {
        if (this is not T here)
            throw new Exception("FunctionalComparable requires both sides to have the same type.");
        
        if (other == null)
            return 1;
        
        foreach (var selector in EnumerateComparators())
        {
            var result = selector(here).CompareTo(selector(other));
            if (result != 0)
                return result;
        }

        return RuntimeHelpers.GetHashCode(this).CompareTo(RuntimeHelpers.GetHashCode(other));
    }
}