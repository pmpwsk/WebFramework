using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace uwap.WebFramework;

/// <summary>
/// Manages the form of a request.
/// </summary>
public class FormManager(HttpContext context)
{
    private HttpContext HttpContext = context;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Data
        => HttpContext.Request.Form;

    /// <summary>
    /// The uploaded files.
    /// </summary>
    public IFormFileCollection Files
        => HttpContext.Request.Form.Files;
}