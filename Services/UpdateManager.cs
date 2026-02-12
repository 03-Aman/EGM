using System;
using System.Threading;
using EGM.Core.Interfaces;
using EGM.Core.Entities;
using EGM.Core.Enums;

namespace EGM.Core.Services
{
    public class UpdateManager : IUpdateManager
    {
        private readonly IConfigManager _configManager;
        private readonly IStateManager _stateManager;
        private readonly ILogger _logger;

        public UpdateManager(IConfigManager configManager, IStateManager stateManager, ILogger logger)
        {
            _configManager = configManager;
            _stateManager = stateManager;
            _logger = logger;
        }

        public void InstallPackage(string packagePath)
        {
            _logger.Log(LogType.Info, $"[Update] Starting install for package: {packagePath}");

            //  Validate State (Must be IDLE to update)
            if (!_stateManager.TransitionTo(EGMStateEnum.UPDATING, "User initiated update"))
            {
                _logger.Log(LogType.Error, "[Update] Cancelled. System must be IDLE.");
                return;
            }

            try
            {
                // backup snapshot
                var currentConfig = _configManager.GetConfig();
                Version backupVersion = currentConfig.CurrentVersion;

                _logger.Log(LogType.Info,
                    $"[Update] Backup created. Last Known Good: {backupVersion}");

                // Simulate Pre-Install Script
                _logger.Log(LogType.Info, "[Update] Running pre-install script...");
                Thread.Sleep(1000);

                // --- SIMULATION LOGIC ---
                if (packagePath.Contains("bad", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Pre-install script failed (Simulated Error).");
                }

                // Increment PATCH version
                var currentVersion = currentConfig.CurrentVersion;

                Version newVersion = new Version( currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);

                _configManager.UpdateConfig(c =>
                {
                    c.LastKnownGoodVersion = backupVersion;
                    c.CurrentVersion = newVersion;
                });

                _logger.Log(LogType.Info, $"[Update] Success! New Version: {newVersion}");
                _stateManager.TransitionTo(EGMStateEnum.IDLE, "Update Complete");

            }
            catch (Exception ex)
            {
              // ROLLBACK LOGIC 
                _logger.Log(LogType.Error, $"[Update] Install Failed: {ex.Message}");
                _logger.Log(LogType.Warning, "[Update] Initiating ROLLBACK...");

                // Restore old version (conceptually)
                // Since we didn't commit the new version yet, we just ensure config is untouched
                // In a real file system, we would copy the backup folder back here.

                _logger.Log(LogType.Info, $"[Update] Rollback successful. Version remains: {_configManager.GetConfig().CurrentVersion}");
                _stateManager.TransitionTo(EGMStateEnum.IDLE, "Update Rolled Back");
            }
        }
    }
}