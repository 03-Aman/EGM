using EGM.Core.Interfaces;
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

                    // Inside ConfigureServices...
                    services.AddSingleton<IBillValidator, BillValidatorService>();
                }).Build();

            // 2. Application Startup
            var logger = host.Services.GetRequiredService<ILogger>();
            var stateManager = host.Services.GetRequiredService<IStateManager>();
            var billValidator = host.Services.GetRequiredService<IBillValidator>(); // Get the service

            logger.Log(LogType.Info, "EGM Core Module Initialized.");
            logger.Log(LogType.Info, $"Current State: {stateManager.CurrentState}");
            // Start the Hardware Monitor
            billValidator.Start();
            //  Keep the console open (Simulation Loop Placeholder)
            Console.WriteLine("System is running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}