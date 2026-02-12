using EGM.Core.Enums;
using EGM.Core.Interfaces;
using EGM.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


    // Setup DI Container (Same as before)
    var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<ILogger, LoggerService>();
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<IStateManager, StateManager>();
            services.AddSingleton<IBillValidator, BillValidatorService>(); // Hardware
            services.AddSingleton<IUpdateManager, UpdateManager>();        // Updates
        })
        .Build();

    //  Resolve Services
    var logger = host.Services.GetRequiredService<ILogger>();
    var stateManager = host.Services.GetRequiredService<IStateManager>();
    var billValidator = host.Services.GetRequiredService<IBillValidator>();
    var updateManager = host.Services.GetRequiredService<IUpdateManager>();
    var configManager = host.Services.GetRequiredService<IConfigManager>();

    //  Start System
    billValidator.Start();
    logger.Log(LogType.Info, "EGM Core Module Ready. Type 'help' for commands.");

    //  The Command Loop
    while (true)
    {
        Console.Write("\nEGM> ");
        string input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();

        try
        {
            switch (command)
            {
                case "exit":
                    billValidator.Stop();
                    return;

                case "help":
                    Console.WriteLine("Commands: start_game, stop_game, signal door_open, update-package <file>, device bill_validator ack on/off, status");
                    break;

                case "status":
                    var cfg = configManager.GetConfig();
                    Console.WriteLine($"State: {stateManager.CurrentState} | Version: {cfg.CurrentVersion} | Last Good: {cfg.LastKnownGoodVersion}");
                    break;

                case "start_game":
                    stateManager.TransitionTo(EGMStateEnum.RUNNING, "User Command");
                    break;

                case "stop_game":
                    stateManager.TransitionTo(EGMStateEnum.IDLE, "User Command");
                    break;

                case "signal":
                    if (parts.Length > 1 && parts[1] == "door_open")
                    {
                        // Door Open -> Immediate Maintenance [cite: 37]
                        stateManager.ForceState(EGMStateEnum.MAINTENANCE, "Door Opened (Security Alert)");
                    }
                    break;

                case "update-package":
                    if (parts.Length > 1) updateManager.InstallPackage(parts[1]);
                    else logger.Log(LogType.Warning, "Usage: update-package <filename>");
                    break;

                case "device":
                    // Handle "device bill_validator ack on/off" [cite: 50-51]
                    if (parts.Length >= 4 && parts[1] == "bill_validator" && parts[2] == "ack")
                    {
                        bool isBroken = (parts[3] == "off");
                        billValidator.SetSimulatedFailure(isBroken);
                    }
                    break;

                case "os":
                    // Handle "os set-timezone Africa/Conakry" [cite: 56]
                    if (parts.Length >= 3 && parts[1] == "set-timezone")
                    {
                        string newZone = parts[2];
                        string oldZone = configManager.GetConfig().TimeZone;

                        configManager.UpdateConfig(c => c.TimeZone = newZone);
                        logger.Audit("Operator", "Set Timezone", oldZone, newZone); // [cite: 63]
                    }
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.Log(LogType.Error, $"Command Error: {ex.Message}");
        }
    }