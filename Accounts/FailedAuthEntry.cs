namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains data about the failed login attempts of an IP.
/// </summary>
public class FailedAuthEntry
{
    /// <summary>
    /// The current amount of failed login attempts.<br/>
    /// Default: 1
    /// </summary>
    public int FailedAttempts = 1;

    /// <summary>
    /// The date and time of the last failed login attempt.<br/>
    /// Default: now
    /// </summary>
    public DateTime LastAttempt = DateTime.UtcNow;
}