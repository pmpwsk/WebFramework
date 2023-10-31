namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Possible results for DMARC.
    /// </summary>
    public enum MailAuthVerdictDMARC
    {
        /// <summary>
        /// DMARC failed and this message should be rejected.
        /// </summary>
        FailWithReject = -3,

        /// <summary>
        /// DMARC failed and this message should go to the spam folder.
        /// </summary>
        FailWithQuarantine = -2,

        /// <summary>
        /// DMARC failed, but the message should be treated as if it had passed.
        /// </summary>
        FailWithoutAction = -1,

        /// <summary>
        /// No valid DMARC record was found for this message.
        /// </summary>
        Unset = 0,

        /// <summary>
        /// This message passed DMARC.
        /// </summary>
        Pass = 1
    }
}