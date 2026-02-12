using EGM.Core.Entities;

namespace EGM.Core.InterFaces
{
    public interface IConfigManager
    {
        SystemConfig GetConfig();
        void UpdateConfig(Action<SystemConfig> updateAction);
    }
}