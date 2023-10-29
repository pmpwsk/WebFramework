namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    public enum MailAuthVerdictSPF
    {
        HardFail = -2,
        SoftFail = -1,
        Unset = 0,
        Pass = 1
    }
}