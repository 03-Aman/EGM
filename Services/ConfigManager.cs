using System;
using System.IO;
using System.Text.Json;
using EGM.Core.Interfaces;
using EGM.Core.Entities;
using EGM.Core.Enums;

namespace EGM.Core.Services
{
    public class ConfigManager : IConfigManager
    {
        private readonly string _configPath;
        private readonly ILogger _logger;
        private SystemConfig _config = new();
        private readonly Lock _lock = new();
        private static readonly JsonSerializerOptions _jsonOptions =
    new JsonSerializerOptions { WriteIndented = true };


        public ConfigManager(ILogger logger)
        {
            _logger = logger;
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _configPath = Path.Combine(dataDir, "config.json");
            LoadConfig();
        }

        public SystemConfig GetConfig() => _config;

        public void UpdateConfig(Action<SystemConfig> updateAction)
        {
            lock (_lock)
            {
                updateAction(_config);
                SaveConfig();
            }
        }

        private void LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<SystemConfig>(json) ?? new SystemConfig();
                    _logger.Log(LogType.Info, "Configuration loaded.");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogType.Error, $"Config load failed: {ex.Message}");
                    _config = new SystemConfig();
                }
            }
            else
            {
                _config = new SystemConfig();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(_config, _jsonOptions);
                File.WriteAllText(_configPath, json);
                _logger.Log(LogType.Info, "Configuration saved.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, $"Config save failed: {ex.Message}");
            }
        }
    }
}