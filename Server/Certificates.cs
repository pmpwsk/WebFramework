using System.Security.Cryptography.X509Certificates;
namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The dictionary for the loaded certificates (key = domain, value = certificate data).
    /// </summary>
    private static readonly Dictionary<string, CertificateEntry> CertificateStore = new();

    /// <summary>
    /// Loads the certificate file at the given path (using the password, if provided) and assigns it to the given domain.<br/>
    /// If another certificate is already assigned to the domain, it will be replaced.
    /// </summary>
    public static void LoadCertificate(string domain, string path, string? password = null)
    {
        if (password == null) CertificateStore[domain] = new CertificateEntry(new X509Certificate2(path), path, null);
        else CertificateStore[domain] = new CertificateEntry(new X509Certificate2(path, password), path, password);
    }

    /// <summary>
    /// Removes deleted certificates, reloads existing ones and attempts to load new certificates from ../Certificates or ../Certificates/Auto (depending on whether AutoCertificate is enabled) without a password.
    /// </summary>
    private static void UpdateCertificates()
    {
        //remove certificates with missing files
        foreach (var pair in CertificateStore)
            if (!File.Exists(pair.Value.Path))
                CertificateStore.Remove(pair.Key);

        //update existing certificates
        foreach (var pair in CertificateStore)
            LoadCertificate(pair.Key, pair.Value.Path, pair.Value.Password);

        //load new certificates without password, if possible
        string directory = Config.AutoCertificate.Email == null ? "../Certificates" : "../Certificates/Auto";
        if (Directory.Exists(directory))
            foreach (string path in Directory.GetFiles(directory, "*.pfx", SearchOption.TopDirectoryOnly))
            {
                string domain = path.Remove(0, path.LastIndexOfAny(new[]{'/', '\\'}) + 1);
                domain = domain.Remove(domain.LastIndexOf('.'));
                if (!CertificateStore.ContainsKey(domain))
                    try
                    {
                        LoadCertificate(domain, path);
                    } catch { }
            }
    }
    
    /// <summary>
    /// Contains data about a certificate.
    /// </summary>
    private class CertificateEntry
    {
        /// <summary>
        /// The certificate object.
        /// </summary>
        internal X509Certificate2 Certificate;

        /// <summary>
        /// The certificate file's path.
        /// </summary>
        internal string Path;

        /// <summary>
        /// The password for the certificate or null if no password was used.
        /// </summary>
        internal string? Password;

        /// <summary>
        /// Creates a new object for data about a certificate.
        /// </summary>
        internal CertificateEntry(X509Certificate2 certificate, string path, string? password)
        {
            Certificate = certificate;
            Path = path;
            Password = password;
        }
    }
}