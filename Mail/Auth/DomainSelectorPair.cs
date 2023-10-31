namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Contains the domain and selector for a DKIM signature.
    /// </summary>
    public readonly struct DomainSelectorPair
    {
        public readonly string Domain, Selector;

        public DomainSelectorPair(string domain, string selector)
        {
            Domain = domain;
            Selector = selector;
        }
    }
}