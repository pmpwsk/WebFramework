using System.Text.Json.Serialization;

namespace uwap.WebFramework.Responses.Dynamic;

public static class ClientActions
{
    /// <summary>
    /// Abstract action sent from the client to the server for dynamic pages.
    /// </summary>
    [JsonDerivedType(typeof(Submit), typeDiscriminator: "Submit")]
    public abstract class AbstractAction;

    /// <summary>
    /// Client action to submit a form.
    /// </summary>
    public class Submit(
        string[] path,
        FormEntry[] data
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly FormEntry[] Data = data;
    }
    
    /// <summary>
    /// Contains a dynamic page path and a string value. 
    /// </summary>
    public class FormEntry(string[] path, string? value)
    {
        public readonly string[] Path = path;
        public readonly string? Value = value;
    }
}