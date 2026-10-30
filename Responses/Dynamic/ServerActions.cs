using System.Text.Json.Serialization;

namespace uwap.WebFramework.Responses.Dynamic;

public static class ServerActions
{
    /// <summary>
    /// Abstract action sent from the server to the client for dynamic pages.
    /// </summary>
    [JsonDerivedType(typeof(Reload), typeDiscriminator: "Reload")]
    [JsonDerivedType(typeof(Welcome), typeDiscriminator: "Welcome")]
    [JsonDerivedType(typeof(FullPage), typeDiscriminator: "FullPage")]
    [JsonDerivedType(typeof(ChangeAttribute), typeDiscriminator: "ChangeAttribute")]
    [JsonDerivedType(typeof(RemoveElement), typeDiscriminator: "RemoveElement")]
    [JsonDerivedType(typeof(AddElementBefore), typeDiscriminator: "AddElementBefore")]
    [JsonDerivedType(typeof(AddElementAfter), typeDiscriminator: "AddElementAfter")]
    [JsonDerivedType(typeof(ChangeContent), typeDiscriminator: "ChangeContent")]
    [JsonDerivedType(typeof(SetValue), typeDiscriminator: "SetValue")]
    [JsonDerivedType(typeof(InternalReload), typeDiscriminator: "InternalReload")]
    [JsonDerivedType(typeof(Navigate), typeDiscriminator: "Navigate")]
    public abstract class AbstractAction;

    /// <summary>
    /// Server action to request a browser reload.
    /// </summary>
    public class Reload : AbstractAction;

    /// <summary>
    /// Server action to welcome the client with the specified watcher ID.
    /// </summary>
    public class Welcome(
        string id
    ) : AbstractAction
    {
        public readonly string Id = id;
    }

    /// <summary>
    /// Server action to send a full page to the client.
    /// </summary>
    public class FullPage(
        List<string> head,
        List<string> beforeScript,
        List<string> afterScript,
        string script
    ) : AbstractAction
    {
        public readonly List<string> Head = head;
        public readonly List<string> BeforeScript = beforeScript;
        public readonly List<string> AfterScript = afterScript;
        public readonly string Script = script;
    }

    /// <summary>
    /// Server action to change an attribute.
    /// </summary>
    public class ChangeAttribute(
        string[] path,
        string attributeName,
        string? attributeValue
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly string AttributeName = attributeName;
        public readonly string? AttributeValue = attributeValue;
    }

    /// <summary>
    /// Server action to remove an element.
    /// </summary>
    public class RemoveElement(
        string[] path
    ) : AbstractAction
    {
        public readonly string[] Path = path;
    }

    /// <summary>
    /// Server action to add an element before a specified one.
    /// </summary>
    public class AddElementBefore(
        string[] path,
        string html
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly string Html = html;
    }

    /// <summary>
    /// Server action to add an element after a specified one.
    /// </summary>
    public class AddElementAfter(
        string[] path,
        string html
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly string Html = html;
    }

    /// <summary>
    /// Server action to change an element's content.
    /// </summary>
    public class ChangeContent(
        string[] path,
        string content
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly string Content = content;
    }

    /// <summary>
    /// Server action to change an element's value.
    /// </summary>
    public class SetValue(
        string[] path,
        object value
    ) : AbstractAction
    {
        public readonly string[] Path = path;
        public readonly object Value = value;
    }

    /// <summary>
    /// Server action to request an internal reload.
    /// </summary>
    public class InternalReload : AbstractAction;

    /// <summary>
    /// Server action to request a navigation to a new location.
    /// </summary>
    public class Navigate(
        string location
    ) : AbstractAction
    {
        public readonly string Location = location;
    }
}