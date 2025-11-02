namespace AMESA_be.Authentication.Config
{
    public class ExternalUserManagement : MainAppSettingsConfig, IPropertiesCloneable<ExternalUserManagement>
    {
        public List<int> DefaultActionsForUser { get; set; }
        public void CloneProperties(ExternalUserManagement cloneFrom)
        {
            DefaultActionsForUser = cloneFrom.DefaultActionsForUser;
        }
    }
}
