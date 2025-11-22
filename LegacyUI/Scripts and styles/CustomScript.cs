namespace uwap.WebFramework.Elements;

/// <summary>
/// IScript with code provided during runtime.
/// </summary>
public class CustomScript : IScript
{
    /// <summary>
    /// The lines of code the script has.
    /// </summary>
    public List<string> Lines;

    /// <summary>
    /// Creates a new custom script with the given (first) line of code.
    /// </summary>
    public CustomScript(string code)
        => Lines = [.. code.Split('\n')];

    /// <summary>
    /// Creates a new custom script with the given lines of code.
    /// </summary>
    public CustomScript(params string[] lines)
        => Lines = [.. lines];

    /// <summary>
    /// Creates a new custom script with the given lines of code.
    /// </summary>
    public CustomScript(List<string> lines)
        => Lines = lines;

    //documentation inherited from IScript
    public IEnumerable<string> Export(Request req)
    {
        yield return "<script>";
        foreach (string line in Lines)
            yield return "\t" + line;
        yield return "</script>";
    }
}