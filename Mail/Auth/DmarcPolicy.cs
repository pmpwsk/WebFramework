namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    private enum DmarcPolicy
    {
        None,
        Quarantine,
        Reject
    }
}