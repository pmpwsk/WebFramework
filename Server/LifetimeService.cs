﻿using Microsoft.Extensions.Hosting;

namespace uwap.WebFramework;

/// <summary>
/// Main class to manage the WebFramework server.
/// </summary>
public static partial class Server
{
    private static readonly CancellationTokenSource StoppingTokenSource = new();

    public static readonly CancellationToken StoppingToken = StoppingTokenSource.Token;

    private class LifetimeService(IHostApplicationLifetime hal) : IHostedService
    {
        private readonly IHostApplicationLifetime HostApplicationLifetime = hal;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            HostApplicationLifetime.ApplicationStarted.Register(ApplicationStarted);
            HostApplicationLifetime.ApplicationStopping.Register(ApplicationStopping);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        private void ApplicationStarted()
        {
            if (Config.Log.Startup)
                Console.WriteLine("Ready for requests.");
            
            if (Config.WorkerInterval >= 0)
                Worker.Change(0, Timeout.Infinite);

            ServerReady?.Invoke();
        }

        private void ApplicationStopping()
        {
            Console.WriteLine("Stopping...");
            PauseRequests = true;
            StoppingTokenSource.Cancel();

            ProgramStopping?.Invoke();
        }
    }
}
