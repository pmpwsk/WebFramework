using SmtpServer;
using System.Net;

namespace uwap.WebFramework.Mail;

public static partial class MailManager
{
    /// <summary>
    /// Manages inbound emails.
    /// </summary>
    public static partial class In
    {
        /// <summary>
        /// The method that decides whether the target of a mail message with the given information is acceptable.<br/>
        /// If there's another reason to not decline the message, the method should throw a SmtpServer.Protocol.SmtpResponseException with one of the static SmtpResponse properties or a custom SmtpResponse.
        /// </summary>
        public static event MailboxExistsDelegate? MailboxExists = null;

        /// <summary>
        /// The method that handles given mail messages (along with the given mail context and authentication result) after they have been accepted by the accepting method.
        /// </summary>
        public static event HandleDelegate? HandleMail = null;

        /// <summary>
        /// The size limit for incoming mail messages in bytes.<br/>
        /// Default: 64MB
        /// </summary>
        public static int SizeLimit { get; set; } = 67108864;

        /// <summary>
        /// Whether to open a listening endpoint for IPv6.<br/>
        /// Default: true
        /// </summary>
        public static bool AllowIPv6 { get; set; } = true;

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
        /// Whether the server has a certificate or not.
        /// </summary>
        public static bool HasCertificate { get; private set; } = false;

        /// <summary>
        /// Starts the server.
        /// </summary>
        public static void Start()
        {
            if (ServerRunning)
                throw new Exception("The server is already running.");
            if (ServerDomain == null)
                throw new Exception("ServerDomain must be set.");
            if (MailboxExists == null)
                throw new Exception("MailboxExists must be set.");
            if (HandleMail == null)
                throw new Exception("HandleMail must be set.");

            var builder = new SmtpServerOptionsBuilder()
                .ServerName(ServerDomain)
                .MaxMessageSize(SizeLimit);

            HasCertificate = WebFramework.Server.GetCertificate(ServerDomain) != null || File.Exists($"../Certificates/Auto/{ServerDomain}.pfx");
            if (HasCertificate)
            {
                builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.Any, 25)).Certificate(new CertificateFactory()));
                if (AllowIPv6)
                    builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.IPv6Any, 25)).Certificate(new CertificateFactory()));
            }
            else
            {
                builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.Any, 25)));
                if (AllowIPv6)
                    builder = builder.Endpoint(b => b.Endpoint(new IPEndPoint(IPAddress.IPv6Any, 25)));
            }

            var options = builder.Build();
            var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            serviceProvider.Add(new Store());
            serviceProvider.Add(new Filter());
            Server = new SmtpServer.SmtpServer(options, serviceProvider);
            ServerTask = Server.StartAsync(CTS.Token);

            if (HasCertificate)
                Console.WriteLine("Mail server started.");
            else Console.WriteLine("Mail server started without a certificate, secure connections will not be possible!");
        }

        /// <summary>
        /// Attempts to start the server and returns whether that action was successful.<br/>
        /// If an exception was thrown while starting, an appropriate line will be written to the console.
        /// </summary>
        public static bool TryStart()
        {
            try
            {
                Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting the mail server: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public static void Stop()
        {
            if (Server == null)
                throw new Exception("The server isn't running.");

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

        /// <summary>
        /// Stops the server, then attempts to start it again and returns whether that action was successful.<br/>
        /// If an exception was thrown while starting, an appropriate line will be written to the console.
        /// </summary>
        public static bool TryRestart()
        {
            Console.WriteLine("Restarting the mail server...");
            Stop();
            return TryStart();
        }
    }
}
