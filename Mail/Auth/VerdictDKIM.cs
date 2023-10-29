namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    public enum MailAuthVerdictDKIM
    {
        Fail = -2,
        Mixed = -1,
        Unset = 0,
        Pass = 1
    }
}