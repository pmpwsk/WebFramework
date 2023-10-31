namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Possible results for DKIM.
    /// </summary>
    public enum MailAuthVerdictDKIM
    {
        /// <summary>
        /// All of the signatures are invalid.
        /// </summary>
        Fail = -2,

        /// <summary>
        /// Some of the signatures are invalid, but some are valid.
        /// </summary>
        Mixed = -1,

        /// <summary>
        /// No signatures found.
        /// </summary>
        Unset = 0,

        /// <summary>
        /// All of the signatures are valid.
        /// </summary>
        Pass = 1
    }
}