using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework;

/// <summary>
/// Intended for POST requests without files.
/// </summary>
public class PostRequest(LayerRequestData data) : SimpleResponseRequest(data)
{
    /// <summary>
    /// Whether the request has set a content type for a form.
    /// </summary>
    public bool HasForm
        => Context.Request.HasFormContentType;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form
        => Context.Request.Form;

    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public async Task<string> GetBodyText()
    {
        using StreamReader reader = new(Context.Request.Body, true);
        try
        {
            return await reader.ReadToEndAsync();
        }
        finally
        {
            reader.Close();
            reader.Dispose();
        }
    }
}