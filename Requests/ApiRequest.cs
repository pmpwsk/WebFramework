using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for backend requests, either to path /api/... or routed over an app request.
/// </summary>
public class ApiRequest : SimpleResponseRequest
{
    /// <summary>
    /// Creates a new API request object with the given context, user, user table and login state.
    /// </summary>
    public ApiRequest(HttpContext context, User? user, UserTable? userTable, LoginState loginState) : base(context, user, userTable, loginState)
    {
    }
}