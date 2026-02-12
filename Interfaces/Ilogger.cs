using EGM.Core.Enums;

namespace EGM.Core.InterFaces
{
    public interface ILogger
    {
        void Log(LogType type, string message);
        void Audit(string actor, string action, string oldValue, string newValue);
    }
}