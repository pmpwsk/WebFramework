using System.Collections.ObjectModel;
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
                    req.UserTable = table;
                    (req.LoginState, var user, var limitedToPaths) = await req.UserTable.AuthenticateAsync(req);
                    if (user != null)
                        req.User = user;
                    string fullPath = req.ProtoHostPath;
                    if (limitedToPaths != null && !limitedToPaths.Any(x => x == fullPath || (x.EndsWith('*') && fullPath.StartsWith(x[..^1]))))
                        return StatusResponse.Forbidden;
                }
            }
            
            return null;
        }
    }
}