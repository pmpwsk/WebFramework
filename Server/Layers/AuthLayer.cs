using uwap.WebFramework.Accounts;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Gets the user table, checks if a user is logged in (along with token management) and redirects to certain authentication pages if necessary.
        /// </summary>
        public static async Task<IResponse?> AuthLayer(Request req)
        {
            if (Config.Accounts.Enabled)
            {
                var table = AccountManager.GetUserTable(req);
                if (table != null)
                {
                    req.UserTableNullable = table;
                    var state = await req.UserTable.AuthenticateAsync(req);
                    string fullPath = req.ProtoHostPath;
                    if (state.LimitedToPaths == null || state.LimitedToPaths.Any(x => x == fullPath || (x.EndsWith('*') && fullPath.StartsWith(x[..^1]))))
                    {
                        req.UserNullable = state.User;
                        req.LoginState = state.LoginState;
                    }
                    else
                    {
                        req.UserNullable = null;
                        req.LoginState = LoginState.None;
                    }
                }
                else
                {
                    req.UserTableNullable = null;
                    req.UserNullable = null;
                    req.LoginState = LoginState.None;
                }
            }
            
            return null;
        }
    }
}