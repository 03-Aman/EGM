using EGM.Core.Interfaces;
using EGM.Core.Enums;
using System.Diagnostics.Eventing.Reader;

namespace EGM.Core.Services
{
    public class UpdateManager : IUpdateManager
    {
        private readonly IConfigManager _config;
        private readonly IStateManager _state;
        private readonly ILogger _logger;
        private readonly IPackageValidator _packageValidator;
        private readonly IInstallHistoryStore _history;

        public UpdateManager( IConfigManager config, IStateManager state,ILogger logger,IPackageValidator validator, IInstallHistoryStore history)
        {
            _config = config;
            _state = state;
            _logger = logger;
            _packageValidator = validator;
            _history = history;
        }

        public void InstallPackage(string packagePath)
        {
            _logger.Log(LogType.Info, $"[Update] Starting install: {packagePath}");

            if (!_state.TransitionTo(EGMStateEnum.UPDATING, "User initiated update"))
            {
                _logger.Log(LogType.Warning, "[Update] System must be IDLE.");
                return;
            }

            Version previousVersion = _config.GetConfig().CurrentVersion;
            if (!_packageValidator.TryValidateAndExtractVersion(packagePath, previousVersion, out Version newVersion, out string erroMessage))
            {
                _logger.Log(LogType.Error, $"[Update] Package validation failed: {erroMessage}");
                _state.TransitionTo(EGMStateEnum.IDLE, "Validation failed");
            }
            else
            {
                try
                {

                    _logger.Log(LogType.Info, $"[Update] Valid package. Target version: {newVersion}");

                    RunPreInstallHook(packagePath);

                    // Commit update
                    _config.UpdateConfig(c =>
                    {
                        c.LastKnownGoodVersion = previousVersion;
                        c.CurrentVersion = newVersion;
                    });

                    // Record history separately
                    _history.RecordInstall(previousVersion, newVersion);

                    _logger.Log(LogType.Info, $"[Update] Success. Version updated to {newVersion}");

                    _state.TransitionTo(EGMStateEnum.IDLE, "Update complete");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogType.Error, $"[Update] Install failed: {ex.Message}");

                    PerformRollback(previousVersion);

                    _state.TransitionTo(EGMStateEnum.IDLE, "Rollback complete");
                }
            }
        }


        private void RunPreInstallHook(string packagePath)
        {
            _logger.Log(LogType.Info, "[Update] Running pre-install script...");

            // Simulated execution delay
            Thread.Sleep(1000);

            if (packagePath.Contains("bad",StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Pre-install script failed.");
            }
        }

        private void PerformRollback(Version previousVersion)
        {
            _logger.Log(LogType.Warning, $"[Update] Rolling back to {previousVersion}");
            _history.RecordRollback(previousVersion);

            _config.UpdateConfig(c =>
            {
                c.CurrentVersion = previousVersion;
            });
        }

    }

}