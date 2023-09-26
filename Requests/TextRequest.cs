using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Abstract class that adds CORS and text responses to IRequest.
/// </summary>
public abstract class TextRequest : IRequest
{
    /// <summary>
    /// Creates a new TextRequest.
    /// </summary>
    public TextRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState)
        : base(context, user, userTable, loginState) { }
}