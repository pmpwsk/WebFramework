using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// A database table to save and manage users.
/// </summary>
public class UserTable : Table<User>
{
    /// <summary>
    /// A cache to quickly find users using their username.
    /// </summary>
    internal Dictionary<string, User> Usernames = new();
    
    /// <summary>
    /// A cache to quickly find users using their mail address.
    /// </summary>
    internal Dictionary<string, User> MailAddresses = new();

    /// <summary>
    /// Creates a new user table with the given name (shouldn't contain characters that are illegal in the target file system).
    /// </summary>
    private UserTable(string name) : base(name) { }

    /// <summary>
    /// Creates a new user table with the given name (shouldn't contain characters that are illegal in the target file system).
    /// </summary>
    protected static new UserTable Create(string name)
    {
        if (!name.All(Tables.KeyChars.Contains)) throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        if (Directory.Exists("../Database/" + name)) throw new Exception("A table with this name already exists, try importing it instead.");
        Directory.CreateDirectory("../Database/" + name);
        UserTable table = new(name);
        Tables.Dictionary[name] = table;
        return table;
    }

    /// <summary>
    /// Loads or creates the/a user table with the given name (shouldn't contain characters that are illegal in the target file system) and returns it.
    /// </summary>
    /// <param name="skipBroken">Whether to skip loading entries that failed to read (otherwise an exception is thrown).</param>
    public static new UserTable Import(string name, bool skipBroken = false)
    {
        if (Tables.Dictionary.TryGetValue(name, out ITable? table)) return (UserTable)table;
        if (!name.All(Tables.KeyChars.Contains)) throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
        if (!Directory.Exists("../Database/" + name)) return Create(name);

        if (Directory.Exists("../Database/Buffer/" + name) && Directory.GetFiles("../Database/Buffer/" + name, "*.json", SearchOption.AllDirectories).Length > 0)
            Console.WriteLine($"The database buffer of table '{name}' contains an entry because a database operation was interrupted. Please manually merge the files and delete the file from the buffer.");

        UserTable result = new(name);
        result.Reload(skipBroken);
        Tables.Dictionary[name] = result;
        return result;
    }

    /// <summary>
    /// Reloads all entries.
    /// </summary>
    /// <param name="skipBroken">Whether to skip loading entries that failed to read (otherwise an exception is thrown).</param>
    public override void Reload(bool skipBroken = false)
    {
        Dictionary<string, TableEntry<User>> data = new();

        foreach (FileInfo file in new DirectoryInfo("../Database/" + Name).EnumerateFiles("*.json"))
        {
            string key = file.Name.Remove(file.Name.Length - 5);
            byte[] json = File.ReadAllBytes(file.FullName);
            try
            {
                User value = Serialization.DeserializeUser(json, out bool updateDatabase);
                if (updateDatabase)
                {
                    json = Serialization.Serialize(value);
                    File.WriteAllBytes(file.FullName, json);
                }
                data[key] = new TableEntry<User>(Name, key, value, json);
                value.ContainingEntry = data[key];
            }
            catch
            {
                if (!skipBroken) throw new Exception($"Key {key} could not be loaded.");
            }
        }

        Data = data;

        Dictionary<string, User> usernames = new(), mailAddresses = new();
        foreach (var entry in Data.Values)
        {
            usernames[entry.Value.Username] = entry.Value;
            mailAddresses[entry.Value.MailAddress] = entry.Value;
        }
        Usernames = usernames;
        MailAddresses = mailAddresses;
    }

    /// <summary>
    /// Gets or sets the user with the given ID.
    /// </summary>
    public override User this[string key]
    {
        get => base[key];
        set
        {
            if (Data.TryGetValue(key, out var oldUser))
            {
                Usernames.Remove(oldUser.Value.Username);
                MailAddresses.Remove(oldUser.Value.MailAddress);
            }
            base[key] = value;
            Usernames[value.Username] = value;
            MailAddresses[value.MailAddress] = value;
        }
    }

    /// <summary>
    /// Removes the user with the given ID if it exists.
    /// </summary>
    public override bool Delete(string key)
    {
        if (!Data.TryGetValue(key, out var entry)) return false;
        Usernames.Remove(entry.Value.Username);
        MailAddresses.Remove(entry.Value.MailAddress);
        base.Delete(key);
        return true;
    }

    /// <summary>
    /// Checks and repairs the cache dictionaries (this is called by the worker).
    /// </summary>
    public void FixAccessories()
    {
        foreach (var kv in Usernames)
            if (kv.Key != kv.Value.Username)
                { Console.WriteLine("Found a username entry with wrong user! This issue has been fixed automatically."); Usernames.Remove(kv.Key); }
            else if ((!TryGetValue(kv.Value.Id, out User? user)) || user != kv.Value)
                { Console.WriteLine("Found a username entry with a deleted or replaced user! This issue has been fixed automatically."); Usernames.Remove(kv.Key); }
        foreach (var kv in MailAddresses)
            if (kv.Key != kv.Value.MailAddress)
                { Console.WriteLine("Found a mail address entry with wrong user! This issue has been fixed automatically."); MailAddresses.Remove(kv.Key); }
            else if ((!TryGetValue(kv.Value.Id, out User? user)) || user != kv.Value)
                { Console.WriteLine("Found a mail address entry with a deleted or replaced user! This issue has been fixed automatically."); MailAddresses.Remove(kv.Key); }

        foreach (var kv in Data)
        {
            if (!Usernames.ContainsKey(kv.Value.Value.Username))
                { Console.WriteLine("Found a user without a username entry! This issue has been fixed automatically."); Usernames[kv.Value.Value.Username] = kv.Value.Value; }
            if (!MailAddresses.ContainsKey(kv.Value.Value.MailAddress))
                { Console.WriteLine("Found a user without a mail address entry! This issue has been fixed automatically."); MailAddresses[kv.Value.Value.MailAddress] = kv.Value.Value; }
        }
    }

    /// <summary>
    /// Checks and returns the login state of the given context and returns the user using the out-parameter if one has been found.
    /// If the user is fully logged in without additional requirements (2FA, verification), the token will be renewed if it's old enough.
    /// </summary>
    public LoginState Authenticate(HttpContext context, out User? user)
    {
        user = null;

        if (AccountManager.IsBanned(context)) return LoginState.Banned;

        if (!context.Request.Cookies.ContainsKey("AuthToken"))
        { //no token present
            return LoginState.None;
        }

        string combinedToken = context.Request.Cookies["AuthToken"] ?? "";
        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12); //the auth token length isn't fixed
        if ((!ContainsKey(id)) || (!this[id].Auth.Exists(authToken)))
        { //user doesn't exist or doesn't contain the token provided
            AccountManager.ReportFailedAuth(context);
            context.Response.Cookies.Delete("AuthToken");
            return LoginState.None;
        }
        AuthTokenData tokenData = this[id].Auth[authToken];
        if (tokenData.Expires < DateTime.UtcNow)
        { //token expired <- don't report this because it's probably not brute-force
            context.Response.Cookies.Delete("AuthToken");
            return LoginState.None;
        }

        user = this[id];
        if (tokenData.Needs2FA) return LoginState.Needs2FA;
        else if (user.MailToken != null) return LoginState.NeedsMailVerification;
        else
        {
            if (tokenData.Expires < DateTime.UtcNow + AccountManager.Settings.TokenExpiration - AccountManager.Settings.TokenRenewalAfter)
            { //renew token if the renewal is due
                AccountManager.AddAuthTokenCookie(user.Id + user.Auth.Renew(authToken), context);
            }
            return LoginState.LoggedIn;
        }
    }

    /// <summary>
    /// Logs out the current client or all other clients.
    /// </summary>
    /// <param name="logoutOthers">Whether to log out all other clients or the current client.</param>
    private void Logout(HttpContext context, bool logoutOthers)
    {
        if (!context.Request.Cookies.ContainsKey("AuthToken")) return;
        string combinedToken = context.Request.Cookies["AuthToken"] ?? "";
        if (!logoutOthers) context.Response.Cookies.Delete("AuthToken", new CookieOptions { Domain = AccountManager.GetWildcardDomain(context.Domain())});
        string id = combinedToken.Remove(12);
        string authToken = combinedToken.Remove(0, 12);
        if (!ContainsKey(id)) return;
        User user = this[id];
        if (logoutOthers)
        {
            user.Auth.DeleteAllExcept(authToken);
        }
        else if (user.Auth.Exists(authToken))
        {
            user.Auth.Delete(authToken);
        }
    }

    /// <summary>
    /// Logs out the current client.
    /// </summary>
    public void Logout(HttpContext context)
        => Logout(context, false);

    /// <summary>
    /// Logs out the current client.
    /// </summary>
    /// <param name="request"></param>
    public void Logout(IRequest request)
        => Logout(request.Context, false);

    /// <summary>
    /// Logs out all other clients.
    /// </summary>
    /// <param name="request"></param>
    public void LogoutOthers(IRequest request)
        => Logout(request.Context, true);

    /// <summary>
    /// Creates and adds a new user using the given data and logs in the given request or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public User Register(string username, string mailAddress, string? password, IRequest request)
    {
        User user = Register(username, mailAddress, password);
        if (request != null) AccountManager.Login(user, request);
        return user;
    }

    /// <summary>
    /// Creates and adds a new user using the given data or throws an Exception if some of the data wasn't acceptable.
    /// </summary>
    public User Register(string username, string mailAddress, string? password)
    {
        if (!AccountManager.CheckUsernameFormat(username)) throw new Exception("Invalid username format.");
        if (!AccountManager.CheckMailAddressFormat(mailAddress)) throw new Exception("Invalid mail address format.");
        if (password != null && !AccountManager.CheckPasswordFormat(password)) throw new Exception("Invalid password format.");

        if (FindByUsername(username) != null) throw new Exception("Another user with the provided username already exists.");
        if (FindByMailAddress(mailAddress) != null) throw new Exception("Another user with the provided email address already exists.");
        User user = new User(username, mailAddress, password, this);
        this[user.Id] = user;
        return user;
    }

    /// <summary>
    /// Logs in the user using the given data or returns null and reports the attempt if the login attempt failed.
    /// </summary>
    public User? Login(string username, string password, IRequest request)
    {
        if (AccountManager.IsBanned(request.Context)) return null;

        User? user = Login(username, password);

        if (user == null) AccountManager.ReportFailedAuth(request.Context);
        else AccountManager.Login(user, request);

        return user;
    }

    /// <summary>
    /// Logs in the user using the given data or returns null if the login attempt failed.
    /// </summary>
    public User? Login(string username, string password)
    {
        if (!AccountManager.CheckUsernameFormat(username)) return null;
        //if (!CheckPasswordFormat(password)) return null;

        User? user = FindByUsername(username);
        if (user == null)
        {
            Password2.WasteTime(password);
            return null;
        }
        else if (user.ValidatePassword(password, null))
            return user;
        else return null;
    }

    /// <summary>
    /// Returns the user with the given username or null if none have been found.
    /// </summary>
    public User? FindByUsername(string username)
    {
        if (Usernames.TryGetValue(username, out var user))
            return user;
        else return null;
    }

    /// <summary>
    /// Returns the user with the given mail address or null if none have been found.
    /// </summary>
    public User? FindByMailAddress(string mailAddress)
    {
        if (MailAddresses.TryGetValue(mailAddress, out var user))
            return user;
        else return null;
    }
}
