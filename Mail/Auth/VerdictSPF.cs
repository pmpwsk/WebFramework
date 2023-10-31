namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Possible results for SPF.
    /// </summary>
    public enum MailAuthVerdictSPF
    {
        /// <summary>
        /// This message should be rejected.
        /// </summary>
        HardFail = -2,

        /// <summary>
        /// This message should go to the spam folder.
        /// </summary>
        SoftFail = -1,

        /// <summary>
        /// No valid SPF record was found for this message.
        /// </summary>
        Unset = 0,

        /// <summary>
        /// This message passed SPF.
        /// </summary>
        Pass = 1
    }
}