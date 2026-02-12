using EGM.Core.Entities;
using EGM.Core.Infrastructure;
using EGM.Core.Interfaces;
using System.Text.Json;

namespace EGM.Core.Persistence
{
    public class InstallHistoryStore : IInstallHistoryStore
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public InstallHistoryStore()
        {
            string dataDir = AppPaths.DataDirectory;
            _filePath = Path.Combine(dataDir, "install_history.json");
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public void RecordInstall(Version previousVersion, Version newVersion)
        {
            var record = new InstallRecord {TimestampUtc = DateTime.UtcNow, PreviousVersion = previousVersion, InstalledVersion = newVersion,RolledBack = false};

            AppendRecord(record);
        }

        public void RecordRollback(Version targetVersion)
        {
            var record = new InstallRecord { TimestampUtc = DateTime.UtcNow, PreviousVersion = targetVersion, InstalledVersion = targetVersion, RolledBack = true};

            AppendRecord(record);
        }

        public IReadOnlyCollection<InstallRecord> GetHistory()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<InstallRecord>>(json) ?? new List<InstallRecord>();
            }
        }

        private void AppendRecord(InstallRecord record)
        {
            lock (_lock)
            {
                var history = GetHistory().ToList();
                history.Add(record);

                string json = JsonSerializer.Serialize(history, _options);
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
