namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    public enum MailAuthVerdictDMARC
    {
        FailWithReject = -3,
        FailWithQuarantine = -2,
        FailWithoutAction = -1,
        Unset = 0,
        Pass = 1
    }
}