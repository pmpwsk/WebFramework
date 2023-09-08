using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

/// <summary>
/// Intended for POST requests without files.
/// </summary>
public class PostRequest : IRequest
{
    /// <summary>
    /// Whether the request has been finished.
    /// </summary>
    private bool Finished = false;

    /// <summary>
    /// The only origin domain the data gotten from the response should be used for (or null to disable).
    /// </summary>
    public string? CorsDomain
    {
        set
        {
            if (value != null) Context.Response.Headers.Add("Access-Control-Allow-Origin", value);
        }
    }

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
    
    /// <summary>
    /// Marks this request as finished and writes a default message for the status code.
    /// </summary>
    public async Task Finish()
    {
        if (Finished) { }
        else
        {
            Finished = true;
            await WriteStatus();
        }
    }
}