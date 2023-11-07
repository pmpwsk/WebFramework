using SmtpServer;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace uwap.WebFramework.Mail;

public static partial class MailManager
{
    /// <summary>
    /// Manages inbound emails.
    /// </summary>
    public static partial class In
    {
        /// <summary>
        /// The method that decides whether a mail message with the given information should be accepted or not.
        /// </summary>
        public static event AcceptDelegate? AcceptMail = null;

        /// <summary>
        /// The method that handles given mail messages (along with the given mail context and authentication result) after they have been accepted by the accepting method.
        /// </summary>
        public static event HandleDelegate? HandleMail = null;

        /// <summary>
        /// The size limit for incoming mail messages in bytes.<br/>
        /// Default: 64MB
        /// </summary>
        public static int SizeLimit = 67108864;

        /// <summary>
        /// Whether to open a listening endpoint for IPv6.<br/>
        /// Default: true
        /// </summary>
        public static bool AllowIPv6 = true;

        /// <summary>
        /// The server object or null if the mail server isn't running.
        /// </summary>
        private static SmtpServer.SmtpServer? Server = null;

        /// <summary>
        /// The task for the server.
        /// </summary>
        private static Task? ServerTask = null;

        /// <summary>
        /// The cancellation token source to stop the server.
        /// </summary>
        private static CancellationTokenSource CTS = new();

        /// <summary>
        /// Whether the server is running or not.
        /// </summary>
        public static bool ServerRunning => ServerTask != null;

        /// <summary>
        /// Starts the server.
        /// </summary>
        public static void Start()
        {
            if (ServerRunning) throw new Exception("The server is already running.");
            if (ServerDomain == null) throw new Exception("ServerDomain must be set.");
            if (AcceptMail == null) throw new Exception("AcceptMail must be set.");
            if (HandleMail == null) throw new Exception("HandleMail must be set.");

            var builder = new SmtpServerOptionsBuilder()
                .ServerName(ServerDomain)
                .MaxMessageSize(SizeLimit);

            X509Certificate2? cert = null;
            try
            {
                if (File.Exists($"../Certificates/Auto/{ServerDomain}.pfx"))
                    cert = new($"../Certificates/Auto/{ServerDomain}.pfx");
                else
                    Console.WriteLine("No certificate found for the mail server domain!");
            }
            catch
            {
                cert = null;
            }
            if (cert == null)
            {
                builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.Any, 25)));
                if (AllowIPv6)
                    builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.IPv6Any, 25)));
            }
            else
            {
                builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.Any, 25)).Certificate(cert));
                if (AllowIPv6)
                    builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.IPv6Any, 25)).Certificate(cert));
            }

            var options = builder.Build();
            var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            serviceProvider.Add(new Store());
            serviceProvider.Add(new Filter());
            Server = new SmtpServer.SmtpServer(options, serviceProvider);
            ServerTask = Server.StartAsync(CTS.Token/*CancellationToken.None*/);

            if (cert != null)
                Console.WriteLine("Mail server started.");
            else Console.WriteLine("Mail server started without a certificate! Secure connections will not be possible!");
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public static void Stop()
        {
            if (Server == null) throw new Exception("The server isn't running.");

            try
            {
                Server.Shutdown();
                Server.ShutdownTask.Wait(1000);
                ServerTask?.Wait(1000);
                CTS.Cancel();
                Console.WriteLine("Stopped the mail server.");
            }
            catch (Exception ex)
            {
                try { CTS.Cancel(); } catch { }
                Console.WriteLine("Error shutting down the mail server gracefully: " + ex.Message);
            }
            finally
            {
                CTS = new();
                Server = null;
                ServerTask?.Dispose();
                ServerTask = null;
            }
        }

        /// <summary>
        /// Stops and starts the mail server.
        /// </summary>
        public static void Restart()
        {
            Console.WriteLine("Restarting the mail server...");
            Stop();
            Start();
        }
    }
}
