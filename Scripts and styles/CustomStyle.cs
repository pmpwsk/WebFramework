namespace uwap.WebFramework.Elements;

/// <summary>
/// IStyle with code provided during runtime.
/// </summary>
public class CustomStyle : IStyle
{
    /// <summary>
    /// The lines of code the style has.
    /// </summary>
    public List<string> Lines;

    /// <summary>
    /// Creates a new custom style with the given (first) line of code.
    /// </summary>
    public CustomStyle(string code)
        => Lines = [.. code.Split('\n')];

    /// <summary>
    /// Creates a new custom style with the given lines of code.
    /// </summary>
    public CustomStyle(params string[] lines)
        => Lines = [.. lines];

    /// <summary>
    /// Creates a new custom style with the given lines of code.
    /// </summary>
    public CustomStyle(List<string> lines)
        => Lines = lines;

    //documentation inherited from IStyle
    public IEnumerable<string> Export(Request req)
    {
        yield return "<style>";
        foreach (string line in Lines)
            yield return "\t" + line;
        yield return "</style>";
    }
}