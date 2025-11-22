namespace uwap.WebFramework;

/// <summary>
/// Interface for web page classes that can be universally exported by the calling middleware.
/// </summary>
public interface IPage
{
    /// <summary>
    /// Enumerates the lines of the generated page.<br/>
    /// In implementations, this should happen without first saving the code as a list of strings or anything similar. It should be generated and sent out immediately.
    /// </summary>
    public IEnumerable<string> Export(Request req);
}