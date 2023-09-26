using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for POST requests without files.
/// </summary>
public class PostRequest : TextRequest
{
    /// <summary>
    /// Creates a new POST request object with the given context, user, user table and login state.
    /// </summary>
    public PostRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
    }

    /// <summary>
    /// The posted form object.
    /// </summary>
    public IFormCollection Form => Context.Request.Form;
}