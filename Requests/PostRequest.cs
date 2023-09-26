using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for POST requests without files.
/// </summary>
public class PostRequest : SimpleResponseRequest
{
    /// <summary>
    /// Creates a new POST request object with the given context, user, user table and login state.
    /// </summary>
    public PostRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
    }

    /// <summary>
    /// Whether the request has set a content type for a form.
    /// </summary>
    public bool HasForm => Context.Request.HasFormContentType;

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form => Context.Request.Form;

    private string? _BodyText = null;
    /// <summary>
    /// The request body, interpreted as text.
    /// </summary>
    public string BodyText
    {
        get
        {
            if (_BodyText == null)
            {
                using StreamReader reader = new(Context.Request.Body, true);
                _BodyText = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            return _BodyText;
        }
    }
}