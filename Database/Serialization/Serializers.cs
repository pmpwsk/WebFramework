namespace uwap.WebFramework.Database;

/// <summary>
/// Contains possible serializers to use.
/// </summary>
public static class Serializers
{
    /// <summary>
    /// Serializer using <c>System.Text.Json</c>.
    /// </summary>
    public static readonly JsonSerializer Json = new();
    
    /// <summary>
    /// Serializer using <c>DataContractSerializer</c>.
    /// </summary>
    public static readonly DataContractJsonSerializer DataContractJson = new();
}