namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Contains the domain and selector for a DKIM signature.
    /// </summary>
    public readonly struct DomainSelectorPair(string domain, string selector)
    {
        public readonly string Domain = domain;

        public readonly string Selector = selector;
    }
}