using AMESA_be.common.Interfaces;

namespace AMESA_be.common.Contracts.SettingsConfig
{
    public class MainAppSettingsConfig : IMainAppSettingsConfig, IPropertiesCloneable<MainAppSettingsConfig>
    {
        public bool UseHealthCheck { get; set; }
        public virtual void CloneProperties(MainAppSettingsConfig cloneFrom)
        {
            UseHealthCheck = cloneFrom.UseHealthCheck;
        }
    }
}
