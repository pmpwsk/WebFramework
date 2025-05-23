﻿using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using uwap.WebFramework.Mail;

namespace uwap.WebFramework;

/// <summary>
/// Main class to manage the WebFramework server.
/// </summary>
public static partial class Server
{
    /// <summary>
    /// ASP.NET object to get the current thread's HttpContext.
    /// </summary>
    private static IHttpContextAccessor? ContextAccessor;
    /// <summary>
    /// The current thread's HttpContext or null if no context is assigned.
    /// </summary>
    public static HttpContext? CurrentHttpContext
        => ContextAccessor?.HttpContext;

    /// <summary>
    /// Whether the server is in debug mode.
    /// </summary>
    public static bool DebugMode { get; set; } = false;

    /// <summary>
    /// Whether the server is currently running.
    /// </summary>
    private static bool Running = false;

    /// <summary>
    /// The ASP.NET web application object.
    /// </summary>
    private static WebApplication? App = null;

    /// <summary>
    /// Starts the web server.<br/>
    /// If local is true, the server will only listen on local IPs so no requests from other computers will be able to reach it.
    /// </summary>
    public static void Start(bool local = false)
    {
        if (Running)
            throw new Exception("Server is already running.");
        if (Config.HttpPort == null && Config.HttpsPort == null && Config.ClientCertificatePort == null)
            throw new Exception("At least one port needs to be set.");
        if (new DirectoryInfo(Directory.GetCurrentDirectory()).Parent == null)
            throw new Exception("The working directory must not be a root directory.");

        if (Config.HttpsPort != null || Config.ClientCertificatePort != null)
        {
            if (Config.Log.Startup)
                Console.WriteLine("Loading certificates...");
            //try loading certificates from ../Certificates without a password
            UpdateCertificates();
        }

        //load cache
        if (Config.Log.Startup)
            Console.WriteLine("Loading cache...");
        UpdateCache();

        if (Config.Log.Startup)
            Console.WriteLine("Configuring server...");

        //configure ports and certificate selector
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            if (Config.HttpPort != null)
            {
                if (local)
                    kestrelOptions.ListenLocalhost((int)Config.HttpPort);
                else kestrelOptions.ListenAnyIP((int)Config.HttpPort);
            }
            
            if (Config.HttpsPort != null)
            {
                if (local)
                    kestrelOptions.ListenLocalhost((int)Config.HttpsPort, lo => ConfigureKestrel(lo, false));
                else kestrelOptions.ListenAnyIP((int)Config.HttpsPort, lo => ConfigureKestrel(lo, false));
            }
            
            if (Config.ClientCertificatePort != null)
            {
                if (local)
                    kestrelOptions.ListenLocalhost((int)Config.ClientCertificatePort, lo => ConfigureKestrel(lo, true));
                else kestrelOptions.ListenAnyIP((int)Config.ClientCertificatePort, lo => ConfigureKestrel(lo, true));
            }

            Config.ConfigureKestrel?.Invoke(kestrelOptions);
        });

        //enable/disable asp.net logs
        builder.Services.AddLogging(Config.Log.AspNet
            ? logging => logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)
            : logging => logging.ClearProviders());

        //context accessor part 1
        builder.Services.AddHttpContextAccessor();

        //configure services according to config
        Config.ConfigureServices?.Invoke(builder.Services);
        
        //add lifetime service
        builder.Services.AddHostedService<LifetimeService>();

        //build
        App = builder.Build();

        //context accessor part 2
        ContextAccessor = App.Services.GetRequiredService<IHttpContextAccessor>();

        //add own middleware
        App.UseMiddleware<Middleware>();
        
        //configure web app according to config
        Config.ConfigureWebApp?.Invoke(App);

        //run :)
        if (Config.Log.Startup)
            Console.WriteLine("Starting server...");
        Running = true;
        App.Run();
    }

    private static void ConfigureKestrel(ListenOptions listenOptions, bool requestCertificates) => listenOptions.UseHttps(httpsOptions =>
    {
        httpsOptions.ServerCertificateSelector = ServerCertificateSelector;

        if (requestCertificates)
            httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
        else if (Config.EnableDelayedClientCertificates)
            httpsOptions.ClientCertificateMode = ClientCertificateMode.DelayCertificate;
    });

    private static X509Certificate2? ServerCertificateSelector(ConnectionContext? _, string? domain)
        => domain != null ? GetCertificate(domain) : null;

    /// <summary>
    /// Creates a new thread to gracefully stop the server and exit the program while finishing the current thread's request.<br/>
    /// If doNotRestart is true, the server will tell the surrounding wrapper (if present) not to restart the program after it exited.
    /// </summary>
    public static void Exit(bool doNotRestart)
    {
        if (doNotRestart)
            Console.WriteLine("wrapper set AutoRestart=false");
        new Thread(Exit).Start();
    }

    /// <summary>
    /// Gracefully stops the server and exits the program.
    /// </summary>
    private static void Exit()
    {
        PauseRequests = true;
        Task.Delay(1000).GetAwaiter().GetResult();
        if (MailManager.In.ServerRunning)
            MailManager.In.Stop();
        App?.StopAsync(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
        Environment.Exit(0);
        Environment.FailFast("Failed to exit softly.");
    }
}
