using EGM.Core.Enums;
using EGM.Core.Interfaces;
using EGM.Core.Persistence;
using EGM.Core.Services;
using EGM.Core.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


// Setup DI Container
using var host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
   {
       services.AddSingleton<ILogger, LoggerService>();
       services.AddSingleton<IConfigManager, ConfigManager>();
       services.AddSingleton<IInstallHistoryStore, InstallHistoryStore>();
       services.AddSingleton<IStateManager, StateManager>();
       services.AddSingleton<IBillValidator, BillValidatorService>();
       services.AddSingleton<IPackageValidator, PackageValidator>();
       services.AddSingleton<ITimeZoneValidator, TimeZoneValidator>();
       services.AddSingleton<IUpdateManager, UpdateManager>();
   }).Build();

//  Resolve Services
var logger = host.Services.GetRequiredService<ILogger>();
    var stateManager = host.Services.GetRequiredService<IStateManager>();
    var billValidator = host.Services.GetRequiredService<IBillValidator>();
    var updateManager = host.Services.GetRequiredService<IUpdateManager>();
    var configManager = host.Services.GetRequiredService<IConfigManager>();
    var timeZoneValidator = host.Services.GetRequiredService<ITimeZoneValidator>();

//  Start System
billValidator.Start();

stateManager.OnStateChanged += (newState) =>
{
    var originalColor = Console.ForegroundColor;

    if (newState == EGMStateEnum.MAINTENANCE)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n[ALERT] SYSTEM ENTERED MAINTENANCE MODE! LOCKING DOWN...");
    }
    else if (newState == EGMStateEnum.RUNNING)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[GAME] Game Started! Good luck!");
    }

    Console.ForegroundColor = originalColor;
};

logger.Log(LogTypeEnum.Info, "EGM Core Module Ready. Type 'help' for commands.");

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
                        // Door Open -> Immediate Maintenance
                        stateManager.ForceState(EGMStateEnum.MAINTENANCE, "Door Opened (Security Alert)");
                    }
                    break;

                case "update-package":
                    if (parts.Length > 1) updateManager.InstallPackage(parts[1].Trim('"'));
                    else logger.Log(LogTypeEnum.Warning, "Usage: update-package <filename>");
                    break;

                case "device":
                // for off : device bill_validator ack off
                // for on : device bill_validator ack on
                if (parts.Length >= 4 && parts[1] == "bill_validator" && parts[2] == "ack")
                    {
                        bool isBroken = (parts[3] == "off");
                        billValidator.SetSimulatedFailure(isBroken);
                    }
                    break;

                case "os":
                    // Handle "os set-timezone Africa/Conakry" 
                    if (parts.Length >= 3 && parts[1] == "set-timezone")
                    {
                        string newZone = parts[2];
                    if (timeZoneValidator.ValidateTimeZone(newZone, out string errorMessage)){
                        string oldZone = configManager.GetConfig().TimeZone;

                        configManager.UpdateConfig(c => c.TimeZone = newZone);
                        logger.Audit("Operator", "Set Timezone", oldZone, newZone); 
                    }
                    else{ 
                        logger.Log(LogTypeEnum.Error, $"Timezone Update Failed: {errorMessage}"); 
                    }
                }
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.Log(LogTypeEnum.Error, $"Command Error: {ex.Message}");
        }
    }