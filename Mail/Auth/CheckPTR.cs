using System.Net;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Checks the given IP address for a PTR record.
    /// </summary>
    public static string? CheckPTR(IPAddress ip)
    {
        var query = MailManager.DnsLookup.QueryReverse(ip);
        var records = query.Answers.PtrRecords();
        var record = records.FirstOrDefault();
        return record?.PtrDomainName?.Original.TrimEnd('.');
    }
}