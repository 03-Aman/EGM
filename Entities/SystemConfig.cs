namespace EGM.Core.Entities
{
    public class SystemConfig
    {
        public string CurrentVersion { get; set; } = "1.0.0";
        public string LastKnownGoodVersion { get; set; } = "0.0.0"; 
        public string TimeZone { get; set; } = "UTC";
        public bool NtpEnabled { get; set; } = true;
    }
}