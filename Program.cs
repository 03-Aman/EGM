using EGM.Core.InterFaces;
using EGM.Core.Enums;
using EGM.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EGM.Core
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {

                    // Register the Logger
                    services.AddSingleton<ILogger, LoggerService>();

                    // Register the Config Manager
                    services.AddSingleton<IConfigManager, ConfigManager>();

                    // Register the State Machine (The Brain)
                    services.AddSingleton<IStateManager, StateManager>();
                })
                .Build();

            // Application Startup (Sanity Check)
            // specific service from the container to start using it
            var logger = host.Services.GetRequiredService<ILogger>();
            var stateManager = host.Services.GetRequiredService<IStateManager>();

            logger.Log(LogType.Info, "EGM Core Module Initialized.");
            logger.Log(LogType.Info, $"Current State: {stateManager.CurrentState}");

            //  Keep the console open (Simulation Loop Placeholder)
            Console.WriteLine("System is running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}