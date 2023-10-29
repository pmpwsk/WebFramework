namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
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