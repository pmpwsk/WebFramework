﻿using NetTools;
using System.Net;
using System.Net.Sockets;

namespace uwap.WebFramework.Mail;

public static partial class MailAuth
{
    /// <summary>
    /// Checks SPF for the given message and returns the domain that passed if the result is "Pass".
    /// </summary>
    public static MailAuthVerdictSPF CheckSPF(string domain, IPAddress ip, out string? passedDomain)
        => CheckSPF(domain, ip, 0, false, out passedDomain);

    /// <summary>
    /// Internal SPF checking, called recursively with limited depth.
    /// </summary>
    private static MailAuthVerdictSPF CheckSPF(string domain, IPAddress ip, int depth, bool isInclude, out string? passedDomain)
    {
        passedDomain = null;

        try
        {
            if (depth >= 10)
                return MailAuthVerdictSPF.Unset;

            var fields = ResolveTXT(domain, "spf1", new[] { new[] { "+all", "-all", "~all", "?all", "a", "mx", "ip4", "ip6", "redirect", "include" } });
            if (fields == null)
                return MailAuthVerdictSPF.Unset;
            foreach (var field in fields)
                if (field.Key == "redirect" && field.Value != null)
                    return CheckSPF(field.Value, ip, depth + 1, false, out passedDomain);

            foreach (var field in fields)
                switch (field.Key)
                {
                    case "+all":
                        if (!isInclude)
                        {
                            passedDomain = domain;
                            return MailAuthVerdictSPF.Pass;
                        }
                        else return MailAuthVerdictSPF.Unset;
                    case "-all":
                        return MailAuthVerdictSPF.HardFail;
                    case "~all":
                        return MailAuthVerdictSPF.SoftFail;
                    case "?all":
                        return MailAuthVerdictSPF.Unset;
                    case "a":
                    case "+a":
                    case "?a":
                        {
                            if (field.Value != null && IPAddress.TryParse(field.Value, out var fieldIP) && MatchAorAAAA(domain, fieldIP))
                            {
                                passedDomain = domain;
                                return MailAuthVerdictSPF.Pass;
                            }
                        }
                        break;
                    case "mx":
                    case "+mx":
                    case "?mx":
                        {
                            var mxQuery = MailManager.DnsLookup.Query(domain, DnsClient.QueryType.MX);
                            var mxRecords = mxQuery.Answers.MxRecords();
                            foreach (var mx in mxRecords)
                                if (IPAddress.TryParse(mx.Exchange, out var mxIP))
                                {
                                    if (ip.Equals(mxIP))
                                    {
                                        passedDomain = domain;
                                        return MailAuthVerdictSPF.Pass;
                                    }
                                }
                                else if (MatchAorAAAA(mx.Exchange, ip))
                                {
                                    passedDomain = domain;
                                    return MailAuthVerdictSPF.Pass;
                                }
                        }
                        break;
                    case "ip4":
                    case "+ip4":
                    case "?ip4":
                    case "ip6":
                    case "+ip6":
                    case "?ip6":
                        {
                            if (field.Value != null && EqualsOrContainsIP(field.Value, ip))
                            {
                                passedDomain = domain;
                                return MailAuthVerdictSPF.Pass;
                            }
                        }
                        break;
                    case "include":
                    case "+include":
                    case "?include":
                        if (field.Value != null && CheckSPF(field.Value, ip, depth + 1, true, out passedDomain) == MailAuthVerdictSPF.Pass)
                            return MailAuthVerdictSPF.Pass;
                        break;

                }

            return MailAuthVerdictSPF.SoftFail;
        }
        catch
        {
            return MailAuthVerdictSPF.Unset;
        }
    }

    /// <summary>
    /// Checks whether a given domain might resolve to the given IP address.
    /// </summary>
    private static bool MatchAorAAAA(string domain, IPAddress ip, int depth = 0)
    {
        try
        {
            if (depth >= 10)
                return false;

            //a or aaaa
            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    var aQuery = MailManager.DnsLookup.Query(domain, DnsClient.QueryType.A);
                    var aRecords = aQuery.Answers.ARecords();
                    foreach (var a in aRecords)
                        if (ip.Equals(a.Address))
                            return true;
                    break;
                case AddressFamily.InterNetworkV6:
                    var aaaaQuery = MailManager.DnsLookup.Query(domain, DnsClient.QueryType.AAAA);
                    var aaaaRecords = aaaaQuery.Answers.AaaaRecords();
                    foreach (var aaaa in aaaaRecords)
                        if (ip.Equals(aaaa.Address))
                            return true;
                    break;
            }

            //cname
            var cnameQuery = MailManager.DnsLookup.Query(domain, DnsClient.QueryType.CNAME);
            var cnameRecords = cnameQuery.Answers.CnameRecords();
            foreach (var cname in cnameRecords)
                if (MatchAorAAAA(cname.CanonicalName, ip, depth + 1))
                    return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether the given string is an IP address or an IP range and whether the given IP equals that or is within the range.
    /// </summary>
    private static bool EqualsOrContainsIP(string addressOrRangeString, IPAddress ip)
    {
        if (IPAddress.TryParse(addressOrRangeString, out var fieldIP))
        {
            if (ip.Equals(fieldIP))
                return true;
        }
        else if (IPAddressRange.TryParse(addressOrRangeString, out var ipRange))
        {
            if (ipRange.Contains(ip))
                return true;
        }
        return false;
    }
}