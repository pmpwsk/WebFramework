using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using OtpNet;
using uwap.WebFramework.Database;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// A database table to save and manage users.
/// </summary>
public class UserTable(string name) : Table<User>(name)
{
    /// <summary>
    /// Index to find users by their username.
    /// </summary>
    private UniqueTableIndex<User, string> UsernameIndex = new(user => user.Username);
    
    /// <summary>
    /// Index to find users by their mail address.
    /// </summary>
    private UniqueTableIndex<User, string> MailAddressIndex = new(user => user.MailAddress);

    protected override IEnumerable<ITableIndex<User>> Indices => [ UsernameIndex, MailAddressIndex ];

    public new static UserTable Import(string name)
        => Tables.Dictionary.TryGetValue(name, out AbstractTable? existingTable) ? (UserTable)existingTable : new UserTable(name);

    /// <summary>
    /// Returns the user with the given username or null if none have been found.
    /// </summary>
    public async Task<User?> FindByUsernameAsync(string username)
        => await GetByIdNullableAsync(await UsernameIndex.GetAsync(username));

    /// <summary>
    /// Returns the user with the given mail address or null if none have been found.
    /// </summary>
    public async Task<User?> FindByMailAddressAsync(string mailAddress)
        => await GetByIdNullableAsync(await MailAddressIndex.GetAsync(mailAddress));

    /// <summary>
    /// Checks and returns the login state of the given request and returns the user using the out-parameter if one has been found.
    /// If the user is fully logged in without additional requirements (2FA, verification), the token will be renewed if it's old enough.
    /// </summary>
    public async Task<(LoginState LoginState, User? User, ReadOnlyCollection<string>? LimitedToPaths)> AuthenticateAsync(Request req)
    {
        if (AccountManager.IsBanned(req))
            return (LoginState.Banned, null, null);

        if (!req.Cookies.TryGetValue("AuthToken", out var combinedToken))
            return (LoginState.None, null, null);

        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12); //the auth token length isn't fixed
        var user = await GetByIdNullableAsync(id);
        if (user == null || !user.Auth.TryGetValue(authToken, out var tokenData))
        { //user doesn't exist or doesn't contain the token provided
            AccountManager.ReportFailedAuth(req);
            req.CookieWriter?.Delete("AuthToken");
            return (LoginState.None, null, null);
        }
        if (tokenData.Expires < DateTime.UtcNow)
        { //token expired <- don't report this because it's probably not brute-force
            req.CookieWriter?.Delete("AuthToken");
            if (Server.Config.Log.AuthTokenExpired)
                Console.WriteLine($"User {id} used an expired auth token.");
            return (LoginState.None, null, null);
        }

        if (tokenData.Needs2FA)
            return (LoginState.Needs2FA, user, tokenData.LimitedToPaths);
        else if (user.MailToken != null)
            return (LoginState.NeedsMailVerification, user, tokenData.LimitedToPaths);
        else
        {
            if (req.CookieWriter != null && tokenData.Expires < DateTime.UtcNow + Server.Config.Accounts.TokenExpiration - Server.Config.Accounts.TokenRenewalAfter)
            { //renew token if the renewal is due
                AccountManager.AddAuthTokenCookie(user.Id + await RenewTokenAsync(user.Id, authToken, tokenData), req, false);
                if (Server.Config.Log.AuthTokenRenewed)
                    Console.WriteLine($"Renewed a token for user {id}.");
            }
            return (LoginState.LoggedIn, user, tokenData.LimitedToPaths);
        }
    }

    /// <summary>
    /// Logs out the current client or all other clients.
    /// </summary>
    /// <param name="req">The request to log out.</param>
    /// <param name="logoutOthers">Whether to log out all other clients or the current client.</param>
    private async Task LogoutAsync(Request req, bool logoutOthers)
    {
        if (!req.Cookies.TryGetValue("AuthToken", out var combinedToken))
            return;
        if (!logoutOthers)
            req.CookieWriter?.Delete("AuthToken", new CookieOptions { Domain = AccountManager.GetWildcardDomain(req.Domain)});
        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12);
        if (!ContainsId(id))
            return;
        if (logoutOthers)
            await DeleteAllTokensExceptAsync(id, authToken);
        else await DeleteTokenAsync(id, authToken);
    }

    /// <summary>
    /// Logs out the current client.
    /// </summary>
    public Task LogoutAsync(Request req)
        => LogoutAsync(req, false);

    /// <summary>
    /// Logs out all other clients.
    /// </summary>
    /// <param name="req"></param>
    public Task LogoutOthersAsync(Request req)
        => LogoutAsync(req, true);

    /// <summary>
    /// Creates and adds a new user using the given data and logs in the given request or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public async Task<User> RegisterAsync(string username, string mailAddress, string? password, Request? req)
    {
        User user = await RegisterAsync(username, mailAddress, password);
        if (req != null)
            await AccountManager.LoginAsync(user, req);
        return user;
    }

    /// <summary>
    /// Creates and adds a new user using the given data or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public async Task<User> RegisterAsync(string username, string mailAddress, string? password)
    {
        if (!AccountManager.CheckUsernameFormat(username))
            throw new Exception("Invalid username format.");
        if (!AccountManager.CheckMailAddressFormat(mailAddress))
            throw new Exception("Invalid mail address format.");
        if (password != null && !AccountManager.CheckPasswordFormat(password))
            throw new Exception("Invalid password format.");

        if (await FindByUsernameAsync(username) != null)
            throw new Exception("Another user with the provided username already exists.");
        if (await FindByMailAddressAsync(mailAddress) != null)
            throw new Exception("Another user with the provided email address already exists.");
        
        User newUser = new(username, mailAddress, password);
        
        return await CreateAsync(12, newUser);
    }

    /// <summary>
    /// Logs in the user using the given data or returns null and reports the attempt if the login attempt failed.
    /// </summary>
    public async Task<User?> LoginAsync(string username, string password, Request req)
    {
        if (AccountManager.IsBanned(req))
            return null;

        User? user = await LoginAsync(username, password);

        if (user == null) AccountManager.ReportFailedAuth(req);
        else await AccountManager.LoginAsync(user, req);

        return user;
    }

    /// <summary>
    /// Logs in the user using the given data or returns null if the login attempt failed.
    /// </summary>
    public async Task<User?> LoginAsync(string username, string password)
    {
        if (!AccountManager.CheckUsernameFormat(username))
            return null;
        //if (!CheckPasswordFormat(password)) return null;

        User? user = await FindByUsernameAsync(username);
        if (user == null)
        {
            Password2.WasteTime(password);
            return null;
        }
        else if (await ValidatePasswordAsync(user.Id, password, null, null))
            return user;
        else return null;
    }
    
    /// <summary>
    /// Sets the username or throws an exception if it has an invalid format, another user is using it or equals the current one.
    /// </summary>
    public Task<User> SetUsernameAsync(string id, string username)
        => AsyncTransactionAsync(id, async t =>
        {
            var oldUsername = t.Value.Username;
            if (!AccountManager.CheckUsernameFormat(username))
                throw new Exception("Invalid username format.");
            if (oldUsername == username)
                throw new Exception("The provided username is the same as the old one.");
            if (await FindByUsernameAsync(username) != null)
                throw new Exception("Another user with the provided username already exists.");
            
            t.Value.Username = username;
        });
    
    /// <summary>
    /// Sets the mail address or throws an exception if its format is invalid or another user is using it or if it equals the current one.
    /// </summary>
    public Task<User> SetMailAddressAsync(string id, string mailAddress)
        => AsyncTransactionAsync(id, async t =>
        {
            var oldAddress = t.Value.MailAddress;
            if (!AccountManager.CheckMailAddressFormat(mailAddress))
                throw new Exception("Invalid mail address format.");
            if (oldAddress == mailAddress)
                throw new Exception("The provided mail address is the same as the old one.");
            if (await FindByMailAddressAsync(mailAddress) != null)
                throw new Exception("Another user with the provided mail address already exists.");
            
            t.Value.MailAddress = mailAddress;
        });
    
    /// <summary>
    /// Sets the password using the default hashing parameters or throws an exception if its format is invalid or if it equals the current one unless it's allowed.
    /// </summary>
    public Task<User> SetPasswordAsync(string id, string? password, bool ignoreRules = false, bool allowSame = false)
        => AsyncTransactionAsync(id, async t =>
        {
            if (password == null)
            {
                t.Value.Password = null;
            }
            else
            {
                if (!ignoreRules && !AccountManager.CheckPasswordFormat(password))
                    throw new Exception("Invalid password format.");
                if (!allowSame && await ValidatePasswordAsync(id, password, null, t.Value))
                    throw new Exception("The provided password is the same as the old one.");

                t.Value.Password = new(password);
            }
        });
    
    public Task<User> SetAccessLevelAsync(string id, ushort accessLevel)
        => TransactionAsync(id, t => t.Value.AccessLevel = accessLevel);

    /// <summary>
    /// Generates, sets and returns a new token for mail verification.
    /// </summary>
    public Task<User> SetNewMailTokenAsync(string id)
        => TransactionAsync(id, t => t.Value.MailToken = Parsers.RandomString(10, false, false, true, t.Value.MailToken));

    /// <summary>
    /// Checks whether the given token is the token for mail verification and sets it to null (=verified) if it's the correct one.
    /// </summary>
    public async Task<bool> VerifyMailAsync(string id, string token, Request? req)
        => (req == null || !AccountManager.IsBanned(req)) && await TransactionAndGetAsync(id, t =>
        {
            if (t.Value.MailToken == null)
                throw new Exception("This user's mail address is already verified.");
            
            if (t.Value.MailToken != token)
            {
                if (req != null)
                    AccountManager.ReportFailedAuth(req);
                return false;
            }
            
            t.Value.MailToken = null;
            
            //does an auth token even exist? (this should be the case, but you never know)
            if (req != null && req.Cookies.TryGetValue("AuthToken", out var combinedToken))
            {
                //get and decode the auth token
                string tokenId = combinedToken.Remove(12);
                string authToken = combinedToken.Remove(0, 12);
                if (t.Value.Id == tokenId && t.Value.Auth.TryGetValue(authToken, out var data))
                {
                    //renew
                    if (Server.Config.Log.AuthTokenRenewed)
                        Console.WriteLine($"Renewed a token after mail verification for user {tokenId}.");
                    AccountManager.AddAuthTokenCookie(tokenId + RenewTokenInTransaction(t.Value, authToken, data), req, false);
                }
            }
            
            return true;
        });

    /// <summary>
    /// Checks whether the given password is correct.
    /// </summary>
    public Task<bool> ValidatePasswordAsync(string id, string password, Request? req)
        => ValidatePasswordAsync(id, password, req, null);
    
    private async Task<bool> ValidatePasswordAsync(string id, string password, Request? req, User? transactionUser)
    {
        var user = transactionUser ?? await GetByIdAsync(id);
        
        if (user.Password == null)
            return false;
        if (req != null && AccountManager.IsBanned(req))
            return false;

        if (user.Password.Check(password))
        {
            if (Server.Config.Accounts.AutoUpgradePasswordHashes && !user.Password.MatchesDefault())
            {
                if (transactionUser != null)
                    user.Password = new(password);
                else await TransactionAsync(id, t => t.Value.Password = new(password));
            }
            return true;
        }

        if (req != null)
            AccountManager.ReportFailedAuth(req);

        return false;
    }

    /// <summary>
    /// Creates a new object to save two-factor authentication data for an account using a new random secret key and new recovery keys. It needs to be verified before it can be used.
    /// </summary>
    public Task<TwoFactorTOTP> GenerateTOTPAsync(string id)
        => TransactionAndGetAsync(id, t => t.Value.TwoFactor.TOTP = new());

    /// <summary>
    /// Disables TOTP 2FA and deletes the corresponding object.
    /// </summary>
    public Task DisableTOTPAsync(string id)
        => TransactionAsync(id, t => t.Value.TwoFactor.TOTP = null);

    /// <summary>
    /// Mark the TOTP 2FA object as verified, enabling it.
    /// </summary>
    public Task VerifyTOTPAsync(string id)
        => TransactionAsync(id, t =>
        {
            if (t.Value.TwoFactor.TOTP != null)
                t.Value.TwoFactor.TOTP.Verified = true;
        });

    /// <summary>
    /// Replaces the list of recovery codes with a newly generated list.
    /// </summary>
    public Task GenerateNewTOTPRecoveryCodesAsync(string id)
        => TransactionAsync(id, t =>
        {
            if (t.Value.TwoFactor.TOTP != null)
                t.Value.TwoFactor.TOTP.Recovery = TwoFactorTOTP.GenerateRecoveryCodes();
        });

    /// <summary>
    /// Checks whether the given code is valid right now and hasn't been used before.
    /// </summary>
    public async Task<bool> ValidateTOTPAsync(string id, string code, Request? req, bool tolerateRecovery)
    {
        var user = await GetByIdAsync(id);
        
        //banned?
        if (req != null && AccountManager.IsBanned(req))
            return false;

        //TOTP disabled?
        if (!user.TwoFactor.TOTPGenerated(out var totp))
            return false;
        
        //recovery code?
        if (tolerateRecovery && totp.RecoveryCodes.Contains(code))
        {
            await TransactionAsync(id, t => t.Value.TwoFactor.TOTP?.RemoveRecoveryCode(code));
            return true;
        }

        //valid and unused?
        if (new Totp(totp.SecretKey).VerifyTotp(code, out long timeStepMatched, new VerificationWindow(1, 1)) && !totp.UsedTimeSteps.Contains(timeStepMatched))
        {
            //add to used time steps (and remove the oldest one if the list is full)
            await TransactionAsync(id, t =>
            {
                t.Value.TwoFactor.TOTP?.AddUsedTimestamp(timeStepMatched);

                //does an auth token even exist? (this should be the case, but you never know)
                if (req != null && req.Cookies.TryGetValue("AuthToken", out var combinedToken))
                {
                    //get and decode the auth token
                    string tokenId = combinedToken.Remove(12);
                    string authToken = combinedToken.Remove(0, 12);
                    if (t.Value.Id == tokenId && t.Value.Auth.TryGetValue(authToken, out var data))
                    {
                        //renew
                        if (Server.Config.Log.AuthTokenRenewed)
                            Console.WriteLine($"Renewed a token after 2FA for user {t.Value.Id}.");
                        AccountManager.AddAuthTokenCookie(t.Value.Id + RenewTokenInTransaction(t.Value, authToken, data), req, false);
                    }
                }
            });
            return true;
        }
        else
        {
            //invalid, report as a failed attempt
            if (req != null)
                AccountManager.ReportFailedAuth(req);
            return false;
        }
    }

    /// <summary>
    /// Removes the given token from the token dictionary, then adds and returns a new token.<br/>
    /// The newly created token will not require 2FA and will not be temporary, so this shouldn't be used for temporary tokens for sessions that aren't fully logged in!
    /// </summary>
    public Task<string> RenewTokenAsync(string id, string oldToken, AuthTokenData data)
        => TransactionAndGetAsync(id, t => RenewTokenInTransaction(t.Value, oldToken, data));
    
    private static string RenewTokenInTransaction(User user, string oldToken, AuthTokenData data)
    {
        if (!user._AuthTokens.Remove(oldToken))
            throw new ArgumentException("The given token isn't present in the token dictionary.");

        string token = Parsers.RandomString(64, user._AuthTokens.Keys);
        user._AuthTokens[token] = new AuthTokenData(false, false, data.FriendlyName, data.LimitedToPaths);
        return token;
    }
    
    public Task SetTokenDataAsync(string id, string authToken, AuthTokenData data)
        => TransactionAsync(id, t => SetTokenDataInTransaction(t.Value, authToken, data));
    
    private static void SetTokenDataInTransaction(User user, string authToken, AuthTokenData data)
    {
        //find and delete least recently used token if there are too many tokens now
        if (!user._AuthTokens.ContainsKey(authToken) && user._AuthTokens.Count >= Server.Config.Accounts.MaxAuthTokens)
        {
            KeyValuePair<string, AuthTokenData>? oldestToken = null;
            foreach (var token in user._AuthTokens)
            {
                if (oldestToken == null || token.Value.Expires < oldestToken.Value.Value.Expires)
                {
                    oldestToken = token;
                }
            }
            if (oldestToken != null)
                user._AuthTokens.Remove(oldestToken.Value.Key);
        }

        user._AuthTokens[authToken] = data;
    }

    /// <summary>
    /// Generates a new authentication token and returns it.<br/>
    /// If the user is using 2FA, the token will still need it before the login process is finished.
    /// </summary>
    public async Task<(string Token, bool Temporary)> AddNewTokenAsync(string id)
    {
        bool temp = false;
        string token = await TransactionAndGetAsync(id, t =>
        {
            string token = Parsers.RandomString(64, t.Value.Auth.ListAll());
            bool twoFactor = t.Value.TwoFactor.TOTPEnabled();
            temp = twoFactor || t.Value.MailToken != null;
            SetTokenDataInTransaction(t.Value, token, new AuthTokenData(twoFactor, temp, null, null));
            return token;
        });
        return (token, temp);
    }

    /// <summary>
    /// Generates a new limited authentication token and returns it.<br/>
    /// The token will not require 2FA and will not be temporary.
    /// </summary>
    public Task<string> AddNewLimitedTokenAsync(string id, string? friendlyName, ReadOnlyCollection<string>? limitedToPaths)
        => TransactionAndGetAsync(id, t =>
        {
            string token = Parsers.RandomString(64, t.Value._AuthTokens.Keys);
            SetTokenDataInTransaction(t.Value, token, new AuthTokenData(false, false, friendlyName, limitedToPaths));
            return token;
        });

    /// <summary>
    /// Deletes the given authentication token if it exists.
    /// </summary>
    public Task DeleteTokenAsync(string id, string authToken)
        => TransactionAsync(id, t => t.Value._AuthTokens.Remove(authToken));

    /// <summary>
    /// Deletes all expired authentication tokens.
    /// </summary>
    internal Task DeleteExpiredTokensAsync(User user)
    {
        if (user._AuthTokens.Any(kv => kv.Value.Expires < DateTime.UtcNow))
            return TransactionAsync(user.Id, t =>
            {
                foreach (var kv in t.Value._AuthTokens.Where(kv => kv.Value.Expires < DateTime.UtcNow).ToList())
                {
                    t.Value._AuthTokens.Remove(kv.Key);
                    if (Server.Config.Log.AuthTokenExpired)
                        Console.WriteLine($"Deleted an expired token for user {user.Id}.");
                }
            });
        else return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes all authentication tokens except the given one (to log out all other clients).
    /// </summary>
    public Task DeleteAllTokensExceptAsync(string id, string authToken)
        => TransactionAsync(id, t =>
        {
            foreach (var kv in t.Value._AuthTokens.Where(kv => kv.Key != authToken).ToList())
                t.Value._AuthTokens.Remove(kv.Key);
        });

    /// <summary>
    /// Deletes all authentication tokens.
    /// </summary>
    public Task DeleteAllTokensAsync(string id)
        => TransactionAsync(id, t => t.Value._AuthTokens.Clear());
    
    public Task SetSettingAsync(string id, string key, string value)
        => TransactionAsync(id, t => t.Value._Settings[key] = value);

    /// <summary>
    /// Deletes the setting with the given key if it exists and returns true if it did.
    /// </summary>
    public Task<bool> DeleteSettingAsync(string id, string key)
        => TransactionAndGetAsync(id, t => t.Value._Settings.Remove(key));
}
