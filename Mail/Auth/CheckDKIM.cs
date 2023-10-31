using MimeKit.Cryptography;
using MimeKit;
using System.Diagnostics.CodeAnalysis;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Checks DKIM for the given message and returns the individual results for each signature (domain and selector).
    /// </summary>
    public static MailAuthVerdictDKIM CheckDKIM(MimeMessage message, out Dictionary<DomainSelectorPair, bool> individualResults)
    {
        try
        {
            individualResults = new();
            var result = MailAuthVerdictDKIM.Unset;

            var verifier = new DkimVerifier(new DkimPublicKeyLocator());

            foreach (var header in message.Headers.Where(x => x.Id == HeaderId.DkimSignature))
            {
                if (!GetDomainAndSelector(header.Value, out string? domain, out string? selector))
                    continue;

                try
                {
                    bool valid = verifier.Verify(message, header);
                    individualResults[new(domain, selector)] = valid;
                    switch (result)
                    {
                        case MailAuthVerdictDKIM.Unset:
                            result = valid ? MailAuthVerdictDKIM.Pass : MailAuthVerdictDKIM.Fail;
                            break;
                        case MailAuthVerdictDKIM.Pass:
                            if (!valid)
                                result = MailAuthVerdictDKIM.Mixed;
                            break;
                        case MailAuthVerdictDKIM.Fail:
                            if (valid)
                                result = MailAuthVerdictDKIM.Mixed;
                            break;
                    }
                }
                catch
                {
                    if (domain != null && selector != null)
                    {
                        individualResults[new(domain, selector)] = false;
                        switch (result)
                        {
                            case MailAuthVerdictDKIM.Unset:
                                result = MailAuthVerdictDKIM.Fail;
                                break;
                            case MailAuthVerdictDKIM.Pass:
                                result = MailAuthVerdictDKIM.Mixed;
                                break;
                        }
                    }
                }
            }

            return result;
        }
        catch
        {
            individualResults = new();
            return MailAuthVerdictDKIM.Unset;
        }
    }

    /// <summary>
    /// Attempts to parse the given DKIM signature to extract the domain and selector from it.
    /// </summary>
    private static bool GetDomainAndSelector(string dkimSignature, [MaybeNullWhen(false)] out string domain, [MaybeNullWhen(false)] out string selector)
    {
        domain = null;
        selector = null;
        foreach (string item in dkimSignature.Split(';'))
        {
            if (item.SplitAtFirst('=', out string? k, out string? v))
            {
                string key = k.Trim();
                string value = v.Trim();
                switch (key)
                {
                    case "d":
                        if (value != "")
                            domain = value;
                        break;
                    case "s":
                        if (selector != "")
                            selector = value;
                        break;
                }
            }
        }
        return domain != null && selector != null;
    }
}