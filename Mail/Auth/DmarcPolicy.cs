namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Possible policies for DMARC.
    /// </summary>
    private enum DmarcPolicy
    {
        /// <summary>
        /// DMARC failures should be ignored.
        /// </summary>
        None,

        /// <summary>
        /// DMARC failures should cause the failing messages to go to the spam folder.
        /// </summary>
        Quarantine,

        /// <summary>
        /// DMARC failures should cause the failing messages to be rejected.
        /// </summary>
        Reject
    }
}