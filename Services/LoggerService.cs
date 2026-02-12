using EGM.Core.InterFaces;
using EGM.Core.Enums; // Added namespace
using System;
using System.IO;

namespace EGM.Core.Services
{
    public class LoggerService : ILogger
    {
        private readonly string _logFilePath;

        public LoggerService()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _logFilePath = Path.Combine(dataDir, "system.log");
        }

        public void Log(LogType type, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string entry = $"[{timestamp}] [{type.ToString().ToUpper()}] {message}";

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = type switch
            {
                LogType.Error => ConsoleColor.Red,
                LogType.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            Console.WriteLine(entry);
            Console.ForegroundColor = originalColor;

            AppendToFile(entry);
        }

        public void Audit(string actor, string action, string oldValue, string newValue)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string entry = $"[{timestamp}] [AUDIT] User: {actor} | Action: {action} | Old: '{oldValue}' -> New: '{newValue}'";

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(entry);
            Console.ForegroundColor = originalColor;

            AppendToFile(entry);
        }

        private void AppendToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
            catch { /* Fail safe */ }
        }
    }
}