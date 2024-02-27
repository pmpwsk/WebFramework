﻿using System.Web;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Plugins;

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

            if (data.UserTable != null)
                data.LoginState = data.UserTable.Authenticate(data.Context, out data.User);
            else
            {
                data.User = null;
                data.LoginState = LoginState.None;
            }


            if (data.Path.StartsWith("/api/") || Config.Accounts.LoginAllowedPaths == null || Config.Accounts.LoginAllowedPaths.Contains(data.Path))
            {
            }
            else if (data.LoginState == LoginState.NeedsMailVerification)
            {
                if (data.Path != Config.Accounts.MailVerifyPath)
                {
                    IPlugin? plugin = PluginManager.GetPlugin(data.Domains, data.Path, out string relPath, out _);
                    if (plugin == null || plugin.GetFileVersion(relPath) == null)
                    {
                        data.Redirect($"{Config.Accounts.MailVerifyPath}?redirect={HttpUtility.UrlEncode(data.Context.PathQuery())}");
                        return Task.FromResult(true);
                    }
                }
            }
            else if (data.LoginState == LoginState.Needs2FA)
            {
                if (data.Path != Config.Accounts.TwoFactorPath)
                {
                    IPlugin? plugin = PluginManager.GetPlugin(data.Domains, data.Path, out string relPath, out _);
                    if (plugin == null || plugin.GetFileVersion(relPath) == null)
                    {
                        data.Redirect($"{Config.Accounts.TwoFactorPath}?redirect={HttpUtility.UrlEncode(data.Context.PathQuery())}");
                        return Task.FromResult(true);
                    }
                }
            }


            return Task.FromResult(false);
        }
    }
}