using System.Security.Cryptography.X509Certificates;
using Certes;
using Certes.Acme;
using Certes.Pkcs;
using uwap.WebFramework.Mail;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// Dictionary for ACME challenge tokens (value) for every domain in the authentication process (key).
    /// </summary>
    private static readonly Dictionary<string,string> AutoCertificateTokens = [];

    /// <summary>
    /// Requests a certificate for the given domain.
    /// </summary>
    private static async Task NewAutoCertificate(string domain)
    {
        if (Config.AutoCertificate.Email == null)
            throw new Exception("The auto certificate email is set to null.");
        
        Directory.CreateDirectory("../Certificates/Auto");

        //check if the server is even reachable over this domain
        string testUrl = $"{domain}/.well-known/acme-challenge/test";
        string testToken = Parsers.RandomString(10);
        AutoCertificateTokens[testUrl] = testToken;
        try
        {
            string testResponse = await new HttpClient().GetStringAsync($"http://{testUrl}");
            AutoCertificateTokens.Remove(testUrl);
            if (testResponse != testToken)
                throw new Exception("Invalid response, aborting.");
        }
        catch
        {
            AutoCertificateTokens.Remove(testUrl);
            if (Config.AutoCertificate.MuteUnreachableErrors)
                return;
            else throw new Exception("The server isn't reachable over the requested domain.");
        }
        
        //login
        AcmeContext acme;
        if (File.Exists("../Certificates/Auto/account.pem"))
        {
            IKey pemKey = KeyFactory.FromPem(File.ReadAllText("../Certificates/Auto/account.pem"));
            acme = new AcmeContext(WellKnownServers.LetsEncryptV2, pemKey);
        }
        else
        {
            acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
            await acme.NewAccount(Config.AutoCertificate.Email, true);
            File.WriteAllText("../Certificates/Auto/account.pem", acme.AccountKey.ToPem());
        }

        //order+authenticate
        IOrderContext order = await acme.NewOrder([domain]);
        IAuthorizationContext authorization = (await order.Authorizations()).First();
        IChallengeContext httpChallenge = await authorization.Http();
        string url = $"{domain}/.well-known/acme-challenge/{httpChallenge.Token}";
        AutoCertificateTokens[url] = httpChallenge.KeyAuthz;

        //validate+export
        await httpChallenge.Validate();
        int failedAttempts = 0;
        while(true)
        {
            try
            {
                await Task.Delay(500);

                IKey privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
                CertificateChain certificate = await order.Generate(new CsrInfo { CommonName = domain }, privateKey);

                string certPem = certificate.ToPem();
                PfxBuilder pfxBuilder = certificate.ToPfx(privateKey);
                byte[] pfx = pfxBuilder.Build(domain, "");
                File.WriteAllBytes($"../Certificates/Auto/{domain}.pfx", pfx);
                AutoCertificateTokens.Remove(url);
                break;
            }
            catch
            {
                if (failedAttempts == 20)
                {
                    AutoCertificateTokens.Remove(url);
                    throw;
                }
                else failedAttempts++;
            }
        }
    }

    /// <summary>
    /// Renews expired automatically generated certificates, requests new ones for newly discovered domains and deletes ones for unused domains.
    /// </summary>
    private async static Task CheckAutoCertificates()
    {
        HashSet<string> domains = [];
        if (Directory.Exists("../Public"))
            foreach (string path in Directory.GetDirectories("../Public", "*", SearchOption.TopDirectoryOnly))
            {
                string domain = path.Remove(0, path.LastIndexOfAny(['/', '\\']) + 1);
                string[] domainSegments = domain.Split('.');
                if (domainSegments.Length > 2 || (domainSegments.Length == 2 && domainSegments[0] != ""))
                    domains.Add(domain);
            }
        foreach (string domain in Config.AutoCertificate.Domains)
            domains.Add(domain);
        foreach (string domain in Config.Domains.Redirect.Keys)
            domains.Add(domain);
        foreach (string domain in PluginManager.GetDomains())
            domains.Add(domain);
        if (MailManager.ServerDomain != null && !domains.Contains(MailManager.ServerDomain))
            domains.Add(MailManager.ServerDomain);

        Directory.CreateDirectory("../Certificates/Auto");

        HashSet<string> coveredDomains = [];

        foreach (string path in Directory.GetFiles("../Certificates/Auto", "*.pfx", SearchOption.TopDirectoryOnly))
        {
            string domain = path.Remove(0, path.LastIndexOfAny(['/', '\\']) + 1);
            domain = domain.Remove(domain.LastIndexOf('.'));
            coveredDomains.Add(domain);
            if (!domains.Contains(domain))
                File.Delete(path);
            else if (DateTime.Parse(X509CertificateLoader.LoadPkcs12FromFile(path, null).GetExpirationDateString()) < (DateTime.UtcNow + TimeSpan.FromDays(30)))
            {
                try
                {
                    await NewAutoCertificate(domain);
                    Console.WriteLine($"Renewed the certificate for {domain}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renewing certificate for {domain}: {ex.Message}");
                }
            }
        }

        foreach (string domain in domains)
        {
            if (coveredDomains.Contains(domain))
                continue;

            try
            {
                await NewAutoCertificate(domain);
                Console.WriteLine($"Received a certificate for {domain}.");
                if (domain == MailManager.ServerDomain && MailManager.In.ServerRunning && !MailManager.In.HasCertificate)
                    MailManager.In.TryRestart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting a certificate for {domain}: {ex.Message}");
            }
        }
    }
}