namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Possible alignments for SPF and DKIM in DMARC.
    /// </summary>
    public enum DmarcAlignment
    {
        /// <summary>
        /// The two domains to be compared must either be the same or one must be a subdomain of the other one (both directions are okay).
        /// </summary>
        Relaxed,

        /// <summary>
        /// The two domains to be compared must be exactly the same.
        /// </summary>
        Strict
    }
}