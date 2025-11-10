using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Settings = uwap.WebFramework.Server.Config.Accounts;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Different states of the login process
/// </summary>
public enum LoginState
{
    /// <summary>
    /// Not logged in and no current login attempt.
    /// </summary>
    None,
    /// <summary>
    /// Properly logged in.
    /// </summary>
    LoggedIn,
    /// <summary>
    /// Logged in with username and password, but 2FA is still required.
    /// </summary>
    Needs2FA,
    /// <summary>
    /// Fully logged in, but the account's email address still needs to be verified.
    /// </summary>
    NeedsMailVerification,
    /// <summary>
    /// Not logged in and banned from making further login attempts for now.
    /// </summary>
    Banned
}

/// <summary>
/// Manages user accounts.
/// </summary>
public static class AccountManager
{
    /// <summary>
    /// The dictionary for failed auth entries for each IP hash that currently has failed attempts.
    /// </summary>
    private static readonly Dictionary<string,FailedAuthEntry> FailedAuth = [];

    /// <summary>
    /// Returns the user table for the domain of the given context (or "any") or returns null if no matches were found.
    /// </summary>
    public static UserTable? GetUserTable(this HttpContext context)
        => Settings.UserTables.TryGetValue(context.Request.Host.Host, out var t1) ? t1 : Settings.UserTables.GetValueOrDefault("any");
    
    /// <summary>
    /// Reports one failed authentication attempt for the requesting IP of the given context.
    /// </summary>
    public static void ReportFailedAuth(HttpContext context)
    {
        if (!Settings.FailedAttempts.EnableBanning)
            return;

        string? ipString = context.IP(); //necessary for ::ffff:
        if (ipString == null)
            return;

        if (!IPAddress.TryParse(ipString, out var ipAddress))
            return;
        byte[] ipBytes = ipAddress.GetAddressBytes();

        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            for (int i = 8; i < ipBytes.Length; i++)
                ipBytes[i] = 0;

        string key = Convert.ToHexString(SHA256.HashData(ipBytes));
        if (FailedAuth.TryGetValue(key, out var fa) && (DateTime.UtcNow-fa.LastAttempt)<Settings.FailedAttempts.BanDuration)
        {
            if (fa.FailedAttempts >= Settings.FailedAttempts.Limit)
                return;
                
            fa.FailedAttempts++;
            fa.LastAttempt = DateTime.UtcNow;
            if (Settings.FailedAttempts.LogBans && fa.FailedAttempts >= Settings.FailedAttempts.Limit)
                Console.WriteLine($"Banned IP \"{new IPAddress(ipBytes)}\" for too many failed authentication attempts.");
        }
        else
        {
            FailedAuth[key] = new FailedAuthEntry();
        }
    }

    /// <summary>
    /// Checks whether the IP of the given context is currently banned from making login attempts.
    /// </summary>
    public static bool IsBanned(HttpContext context)
    {
        string? ip = context.IP();
        if (ip == null) return false;
        string key = Convert.ToHexString(ip.ToSha256());
        if (FailedAuth.TryGetValue(key, out var fa) && (DateTime.UtcNow-fa.LastAttempt)<Settings.FailedAttempts.BanDuration)
            return fa.FailedAttempts >= Settings.FailedAttempts.Limit;
        else return false;
    }

    /// <summary>
    /// Deletes all expired failed authentication bans.
    /// </summary>
    public static void DeleteExpiredBans()
    {
        foreach (var kv in FailedAuth)
            if (DateTime.UtcNow - FailedAuth[kv.Key].LastAttempt >= Settings.FailedAttempts.BanDuration)
                FailedAuth.Remove(kv.Key);
    }

    /// <summary>
    /// Adds a cookie for the given authentication token to the given context.
    /// </summary>
    internal static void AddAuthTokenCookie(string combinedToken, HttpContext context, bool temporary)
    {
        GenerateAuthTokenCookieOptions(out var expires, out var sameSite, out var domain, context, temporary);
        context.Response.Cookies.Append("AuthToken", combinedToken, new CookieOptions()
        {
            Expires = expires,
            SameSite = sameSite,
            HttpOnly = Settings.HttpOnly,
            Path = "/",
            Domain = domain
        });
    }

    /// <summary>
    /// Generates the appropriate cookie options.
    /// </summary>
    public static void GenerateAuthTokenCookieOptions(out DateTime expires, out SameSiteMode sameSite, out string? domain, HttpContext context, bool temporary = false)
    {
        expires = DateTime.UtcNow + (temporary ? TimeSpan.FromMinutes(10) : Settings.TokenExpiration);
        sameSite = Settings.SameSiteStrict ? SameSiteMode.Strict : SameSiteMode.Lax;
        domain = GetWildcardDomain(context.Domain());
    }

    /// <summary>
    /// Returns the auth cookie wildcard domain to be used for the given domain or null if no matching domain was set.
    /// </summary>
    public static string? GetWildcardDomain(string domain)
    {
        string? wildcard;
        if (Settings.WildcardDomains.Contains(domain))
            wildcard = domain;
        else wildcard = Settings.WildcardDomains.FirstOrDefault(x => domain.EndsWith('.' + x) && !domain[..(domain.Length - x.Length - 1)].Contains('.'));

        if (wildcard != null) return '.' + wildcard;
        else return null;
    }

    /// <summary>
    /// Creates a new authentication token for the given user and adds a cookie for it to the given context.
    /// </summary>
    internal static void Login(User user, Request req)
        => AddAuthTokenCookie(user.Id + req.UserTable.AddNewToken(user.Id, out bool temporary), req.Context, temporary);

    /// <summary>
    /// Check whether the given username satisfies the username requirements.
    /// </summary>
    public static bool CheckUsernameFormat(string username)
    {
        if (username.Length < 3) return false;
        if ("-._".Contains(username.First()) || "-._".Contains(username.Last())) return false;
        string supportedChars = "abcdefghijklmnopqrstuvwxyz0123456789-._";
        return username.All(x => supportedChars.Contains(x));
    }

    /// <summary>
    /// Check whether the given password satisfies the password requirements.
    /// </summary>
    public static bool CheckPasswordFormat(string password)
    {
        if (password.Length < 8) return false;
        string capitalChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        string digitChars = "0123456789";
        bool capital = false, lower = false, digit = false, special = false;
        foreach (char c in password)
        {
            if (capitalChars.Contains(c)) capital = true;
            else if (lowerChars.Contains(c)) lower = true;
            else if (digitChars.Contains(c)) digit = true;
            else special = true;
        }
        return capital && lower && digit && special;
    }

    /// <summary>
    /// Check whether the given mail address satisfies the mail address requirements.
    /// </summary>
    public static bool CheckMailAddressFormat(string mailAddress)
    {
        if (mailAddress.StartsWith('@')) return false;
        bool atPassed = false;
        string domain = "";
        foreach (char c in mailAddress)
        {
            if (c == '@')
            {
                if (atPassed) return false;
                else atPassed = true;
            }
            else if (atPassed) domain += c;
        }
        return domain.Contains('.') && !(domain.StartsWith('.') || domain.EndsWith('.'));
    }
}