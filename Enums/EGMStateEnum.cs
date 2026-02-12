
namespace EGM.Core.Enums
{
    public enum EGMStateEnum
    {
        IDLE,           // Default state
        RUNNING,        // Game is active
        MAINTENANCE,    // Door open or hardware failure [cite: 37, 46]
        UPDATING,       // Installing package [cite: 72]
        ERROR           // Generic error state [cite: 73]
    }
}
