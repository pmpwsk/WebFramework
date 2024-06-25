using System.Collections.ObjectModel;
using uwap.WebFramework.Accounts;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        /// <summary>
        /// Gets the user table, checks if a user is logged in (along with token management) and redirects to certain authentication pages if necessary.
        /// </summary>
        public static Task<bool> AuthLayer(LayerRequestData data)
        {
            data.UserTable = Config.Accounts.Enabled ? AccountManager.GetUserTable(data.Context) : null;

            ReadOnlyCollection<string>? limitedToPaths = null;
            if (data.UserTable != null)
                data.LoginState = data.UserTable.Authenticate(data.Context, out data.User, out limitedToPaths);
            else
            {
                data.User = null;
                data.LoginState = LoginState.None;
            }

            string fullPath = data.Domain + data.Path;
            if (limitedToPaths != null && !limitedToPaths.Any(x => x == fullPath || (x.EndsWith('*') && fullPath.StartsWith(x[..^1]))))
            {
                data.Status = 403;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}