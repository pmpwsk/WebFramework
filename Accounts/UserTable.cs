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
    public User? FindByUsername(string username)
        => GetByIdNullable(UsernameIndex.Get(username));

    /// <summary>
    /// Returns the user with the given mail address or null if none have been found.
    /// </summary>
    public User? FindByMailAddress(string mailAddress)
        => GetByIdNullable(MailAddressIndex.Get(mailAddress));

    /// <summary>
    /// Checks and returns the login state of the given context and returns the user using the out-parameter if one has been found.
    /// If the user is fully logged in without additional requirements (2FA, verification), the token will be renewed if it's old enough.
    /// </summary>
    public LoginState Authenticate(HttpContext context, out User? user, out ReadOnlyCollection<string>? limitedToPaths)
    {
        if (AccountManager.IsBanned(context))
        {
            user = null;
            limitedToPaths = null;
            return LoginState.Banned;
        }

        if (!context.Request.Cookies.TryGetValue("AuthToken", out var combinedToken))
        { //no token present
            user = null;
            limitedToPaths = null;
            return LoginState.None;
        }

        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12); //the auth token length isn't fixed
        if (!TryGetValue(id, out user) || !user.Auth.TryGetValue(authToken, out var tokenData))
        { //user doesn't exist or doesn't contain the token provided
            AccountManager.ReportFailedAuth(context);
            context.Response.Cookies.Delete("AuthToken");
            user = null;
            limitedToPaths = null;
            return LoginState.None;
        }
        if (tokenData.Expires < DateTime.UtcNow)
        { //token expired <- don't report this because it's probably not brute-force
            context.Response.Cookies.Delete("AuthToken");
            user = null;
            if (Server.Config.Log.AuthTokenExpired)
                Console.WriteLine($"User {id} used an expired auth token.");
            limitedToPaths = null;
            return LoginState.None;
        }

        limitedToPaths = tokenData.LimitedToPaths;
        if (tokenData.Needs2FA)
            return LoginState.Needs2FA;
        else if (user.MailToken != null)
            return LoginState.NeedsMailVerification;
        else
        {
            if (tokenData.Expires < DateTime.UtcNow + Server.Config.Accounts.TokenExpiration - Server.Config.Accounts.TokenRenewalAfter)
            { //renew token if the renewal is due
                AccountManager.AddAuthTokenCookie(user.Id + RenewToken(user.Id, authToken, tokenData), context, false);
                if (Server.Config.Log.AuthTokenRenewed)
                    Console.WriteLine($"Renewed a token for user {id}.");
            }
            return LoginState.LoggedIn;
        }
    }

    /// <summary>
    /// Logs out the current client or all other clients.
    /// </summary>
    /// <param name="context">The context to log out.</param>
    /// <param name="logoutOthers">Whether to log out all other clients or the current client.</param>
    private void Logout(HttpContext context, bool logoutOthers)
    {
        if (!context.Request.Cookies.TryGetValue("AuthToken", out var combinedToken))
            return;
        if (!logoutOthers)
            context.Response.Cookies.Delete("AuthToken", new CookieOptions { Domain = AccountManager.GetWildcardDomain(context.Domain())});
        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12);
        if (!ContainsId(id))
            return;
        if (logoutOthers)
            DeleteAllTokensExcept(id, authToken);
        else DeleteToken(id, authToken);
    }

    /// <summary>
    /// Logs out the current client.
    /// </summary>
    public void Logout(HttpContext context)
        => Logout(context, false);

    /// <summary>
    /// Logs out the current client.
    /// </summary>
    /// <param name="req"></param>
    public void Logout(Request req)
        => Logout(req.Context, false);

    /// <summary>
    /// Logs out all other clients.
    /// </summary>
    /// <param name="req"></param>
    public void LogoutOthers(Request req)
        => Logout(req.Context, true);

    /// <summary>
    /// Creates and adds a new user using the given data and logs in the given request or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public User Register(string username, string mailAddress, string? password, Request? req)
    {
        User user = Register(username, mailAddress, password);
        if (req != null)
            AccountManager.Login(user, req);
        return user;
    }

    /// <summary>
    /// Creates and adds a new user using the given data or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public User Register(string username, string mailAddress, string? password)
    {
        if (!AccountManager.CheckUsernameFormat(username))
            throw new Exception("Invalid username format.");
        if (!AccountManager.CheckMailAddressFormat(mailAddress))
            throw new Exception("Invalid mail address format.");
        if (password != null && !AccountManager.CheckPasswordFormat(password))
            throw new Exception("Invalid password format.");

        if (FindByUsername(username) != null)
            throw new Exception("Another user with the provided username already exists.");
        if (FindByMailAddress(mailAddress) != null)
            throw new Exception("Another user with the provided email address already exists.");
        
        User newUser = new(username, mailAddress, password);
        
        TransactionNullable(Parsers.RandomString(12, id => !ContainsId(id)), (ref User? user) => user = newUser);
        return newUser;
    }

    /// <summary>
    /// Logs in the user using the given data or returns null and reports the attempt if the login attempt failed.
    /// </summary>
    public User? Login(string username, string password, Request req)
    {
        if (AccountManager.IsBanned(req.Context))
            return null;

        User? user = Login(username, password);

        if (user == null) AccountManager.ReportFailedAuth(req.Context);
        else AccountManager.Login(user, req);

        return user;
    }

    /// <summary>
    /// Logs in the user using the given data or returns null if the login attempt failed.
    /// </summary>
    public User? Login(string username, string password)
    {
        if (!AccountManager.CheckUsernameFormat(username))
            return null;
        //if (!CheckPasswordFormat(password)) return null;

        User? user = FindByUsername(username);
        if (user == null)
        {
            Password2.WasteTime(password);
            return null;
        }
        else if (ValidatePassword(user.Id, password, null, null))
            return user;
        else return null;
    }
    
    /// <summary>
    /// Sets the username or throws an exception if it has an invalid format, another user is using it or equals the current one.
    /// </summary>
    public User SetUsername(string id, string username)
        => Transaction(id, (ref User user) =>
        {
            var oldUsername = user.Username;
            if (!AccountManager.CheckUsernameFormat(username))
                throw new Exception("Invalid username format.");
            if (oldUsername == username)
                throw new Exception("The provided username is the same as the old one.");
            if (FindByUsername(username) != null)
                throw new Exception("Another user with the provided username already exists.");
            
            user.Username = username;
        });
    
    /// <summary>
    /// Sets the mail address or throws an exception if its format is invalid or another user is using it or if it equals the current one.
    /// </summary>
    public User SetMailAddress(string id, string mailAddress)
        => Transaction(id, (ref User user) =>
        {
            var oldAddress = user.MailAddress;
            if (!AccountManager.CheckMailAddressFormat(mailAddress))
                throw new Exception("Invalid mail address format.");
            if (oldAddress == mailAddress)
                throw new Exception("The provided mail address is the same as the old one.");
            if (FindByMailAddress(mailAddress) != null)
                throw new Exception("Another user with the provided mail address already exists.");
            
            user.MailAddress = mailAddress;
        });
    
    /// <summary>
    /// Sets the password using the default hashing parameters or throws an exception if its format is invalid or if it equals the current one unless it's allowed.
    /// </summary>
    public User SetPassword(string id, string? password, bool ignoreRules = false, bool allowSame = false)
        => Transaction(id, (ref User user) =>
        {
            if (password == null)
            {
                user.Password = null;
            }
            else
            {
                if (!ignoreRules && !AccountManager.CheckPasswordFormat(password))
                    throw new Exception("Invalid password format.");
                if (!allowSame && ValidatePassword(id, password, null, user))
                    throw new Exception("The provided password is the same as the old one.");

                user.Password = new(password);
            }
        });
    
    public User SetAccessLevel(string id, ushort accessLevel)
        => Transaction(id, (ref User user) => user.AccessLevel = accessLevel);

    /// <summary>
    /// Generates, sets and returns a new token for mail verification.
    /// </summary>
    public User SetNewMailToken(string id)
        => Transaction(id, (ref User user) => user.MailToken = Parsers.RandomString(10, false, false, true, user.MailToken));

    /// <summary>
    /// Checks whether the given token is the token for mail verification and sets it to null (=verified) if it's the correct one.
    /// </summary>
    public bool VerifyMail(string id, string token, Request? req)
        => (req == null || !AccountManager.IsBanned(req.Context)) && TransactionAndGet(id, (ref User user) =>
        {
            if (user.MailToken == null)
                throw new Exception("This user's mail address is already verified.");
            
            if (user.MailToken != token)
            {
                if (req != null)
                    AccountManager.ReportFailedAuth(req.Context);
                return false;
            }
            
            user.MailToken = null;
            
            //does an auth token even exist? (this should be the case, but you never know)
            if (req != null && req.Cookies.TryGetValue("AuthToken", out var combinedToken))
            {
                //get and decode the auth token
                string tokenId = combinedToken.Remove(12);
                string authToken = combinedToken.Remove(0, 12);
                if (user.Id == tokenId && user.Auth.TryGetValue(authToken, out var data))
                {
                    //renew
                    if (Server.Config.Log.AuthTokenRenewed)
                        Console.WriteLine($"Renewed a token after mail verification for user {tokenId}.");
                    AccountManager.AddAuthTokenCookie(tokenId + RenewTokenInTransaction(user, authToken, data), req.Context, false);
                }
            }
            
            return true;
        });

    /// <summary>
    /// Checks whether the given password is correct.
    /// </summary>
    public bool ValidatePassword(string id, string password, Request? req)
        => ValidatePassword(id, password, req, null);
    
    private bool ValidatePassword(string id, string password, Request? req, User? transactionUser)
    {
        var user = transactionUser ?? GetById(id);
        
        if (user.Password == null)
            return false;
        if (req != null && AccountManager.IsBanned(req.Context))
            return false;

        if (user.Password.Check(password))
        {
            if (Server.Config.Accounts.AutoUpgradePasswordHashes && !user.Password.MatchesDefault())
            {
                if (transactionUser != null)
                    user.Password = new(password);
                else Transaction(id, (ref User u) => u.Password = new(password));
            }
            return true;
        }

        if (req != null)
            AccountManager.ReportFailedAuth(req.Context);

        return false;
    }

    /// <summary>
    /// Creates a new object to save two-factor authentication data for an account using a new random secret key and new recovery keys. It needs to be verified before it can be used.
    /// </summary>
    public void GenerateTOTP(string id)
        => Transaction(id, (ref User user) => user.TwoFactor.TOTP = new());

    /// <summary>
    /// Disables TOTP 2FA and deletes the corresponding object.
    /// </summary>
    public void DisableTOTP(string id)
        => Transaction(id, (ref User user) => user.TwoFactor.TOTP = null);

    /// <summary>
    /// Mark the TOTP 2FA object as verified, enabling it.
    /// </summary>
    public void VerifyTOTP(string id)
        => Transaction(id, (ref User user) =>
        {
            if (user.TwoFactor.TOTP != null)
                user.TwoFactor.TOTP.Verified = true;
        });

    /// <summary>
    /// Replaces the list of recovery codes with a newly generated list.
    /// </summary>
    public void GenerateNewTOTPRecoveryCodes(string id)
        => Transaction(id, (ref User user) =>
        {
            if (user.TwoFactor.TOTP != null)
                user.TwoFactor.TOTP.Recovery = TwoFactorTOTP.GenerateRecoveryCodes();
        });

    /// <summary>
    /// Checks whether the given code is valid right now and hasn't been used before.
    /// </summary>
    public bool ValidateTOTP(string id, string code, Request? req, bool tolerateRecovery)
    {
        var user = GetById(id);
        
        //banned?
        if (req != null && AccountManager.IsBanned(req.Context))
            return false;

        //TOTP disabled?
        if (!user.TwoFactor.TOTPEnabled(out var totp))
            return false;
        
        //recovery code?
        if (tolerateRecovery && totp.RecoveryCodes.Contains(code))
        {
            Transaction(id, (ref User u) => u.TwoFactor.TOTP?.RemoveRecoveryCode(code));
            return true;
        }

        //valid and unused?
        if (new Totp(totp.SecretKey).VerifyTotp(code, out long timeStepMatched, new VerificationWindow(1, 1)) && !totp.UsedTimeSteps.Contains(timeStepMatched))
        {
            //add to used time steps (and remove the oldest one if the list is full)
            Transaction(id, (ref User u) =>
            {
                u.TwoFactor.TOTP?.AddUsedTimestamp(timeStepMatched);

                //does an auth token even exist? (this should be the case, but you never know)
                if (req != null && req.Cookies.TryGetValue("AuthToken", out var combinedToken))
                {
                    //get and decode the auth token
                    string tokenId = combinedToken.Remove(12);
                    string authToken = combinedToken.Remove(0, 12);
                    if (u.Id == tokenId && u.Auth.TryGetValue(authToken, out var data))
                    {
                        //renew
                        if (Server.Config.Log.AuthTokenRenewed)
                            Console.WriteLine($"Renewed a token after 2FA for user {u.Id}.");
                        AccountManager.AddAuthTokenCookie(u.Id + RenewTokenInTransaction(u, authToken, data), req.Context, false);
                    }
                }
            });
            return true;
        }
        else
        {
            //invalid, report as a failed attempt
            if (req != null)
                AccountManager.ReportFailedAuth(req.Context);
            return false;
        }
    }

    /// <summary>
    /// Removes the given token from the token dictionary, then adds and returns a new token.<br/>
    /// The newly created token will not require 2FA and will not be temporary, so this shouldn't be used for temporary tokens for sessions that aren't fully logged in!
    /// </summary>
    public string RenewToken(string id, string oldToken, AuthTokenData data)
        => TransactionAndGet(id, (ref User user) => RenewTokenInTransaction(user, oldToken, data));
    
    private static string RenewTokenInTransaction(User user, string oldToken, AuthTokenData data)
    {
        if (!user._AuthTokens.Remove(oldToken))
            throw new ArgumentException("The given token isn't present in the token dictionary.");

        string token = Parsers.RandomString(64, user._AuthTokens.Keys);
        user._AuthTokens[token] = new AuthTokenData(false, false, data.FriendlyName, data.LimitedToPaths);
        return token;
    }
    
    public void SetTokenData(string id, string authToken, AuthTokenData data)
        => Transaction(id, (ref User user) => SetTokenDataInTransaction(user, authToken, data));
    
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
    public string AddNewToken(string id, out bool temporary)
    {
        bool temp = false;
        string token = TransactionAndGet(id, (ref User user) =>
        {
            string token = Parsers.RandomString(64, user.Auth.ListAll());
            bool twoFactor = user.TwoFactor.TOTPEnabled();
            temp = twoFactor || user.MailToken != null;
            SetTokenDataInTransaction(user, token, new AuthTokenData(twoFactor, temp, null, null));
            return token;
        });
        temporary = temp;
        return token;
    }

    /// <summary>
    /// Generates a new limited authentication token and returns it.<br/>
    /// The token will not require 2FA and will not be temporary.
    /// </summary>
    public string AddNewLimitedToken(string id, string? friendlyName, ReadOnlyCollection<string>? limitedToPaths)
        => TransactionAndGet(id, (ref User user) =>
        {
            string token = Parsers.RandomString(64, user._AuthTokens.Keys);
            SetTokenDataInTransaction(user, token, new AuthTokenData(false, false, friendlyName, limitedToPaths));
            return token;
        });

    /// <summary>
    /// Deletes the given authentication token if it exists.
    /// </summary>
    public void DeleteToken(string id, string authToken)
        => Transaction(id, (ref User user) => user._AuthTokens.Remove(authToken));

    /// <summary>
    /// Deletes all expired authentication tokens.
    /// </summary>
    internal void DeleteExpiredTokens(User user)
    {
        if (user._AuthTokens.Any(kv => kv.Value.Expires < DateTime.UtcNow))
            Transaction(user.Id, (ref User u) =>
            {
                foreach (var kv in u._AuthTokens.Where(kv => kv.Value.Expires < DateTime.UtcNow).ToList())
                {
                    u._AuthTokens.Remove(kv.Key);
                    if (Server.Config.Log.AuthTokenExpired)
                        Console.WriteLine($"Deleted an expired token for user {user.Id}.");
                }
            });
    }

    /// <summary>
    /// Deletes all authentication tokens except the given one (to log out all other clients).
    /// </summary>
    public void DeleteAllTokensExcept(string id, string authToken)
        => Transaction(id, (ref User user) =>
        {
            foreach (var kv in user._AuthTokens.Where(kv => kv.Key != authToken).ToList())
                user._AuthTokens.Remove(kv.Key);
        });

    /// <summary>
    /// Deletes all authentication tokens.
    /// </summary>
    public void DeleteAllTokens(string id)
        => Transaction(id, (ref User user) => user._AuthTokens.Clear());
    
    public void SetSetting(string id, string key, string value)
        => Transaction(id, (ref User user) => user._Settings[key] = value);

    /// <summary>
    /// Deletes the setting with the given key if it exists and returns true if it did.
    /// </summary>
    public bool DeleteSetting(string id, string key)
        => TransactionAndGet(id, (ref User user) => user._Settings.Remove(key));
}
