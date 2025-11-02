using AMESA_be.common.Interfaces;

namespace AMESA_be.common.Config
{
    public class HttpClientsConfig : HttpClientConfig, IPropertiesCloneable<HttpClientsConfig>
    {
        public Dictionary<string, HttpClientConfig> Clients { get; set; }

        public void CloneProperties(HttpClientsConfig cloneFrom)
        {
            base.CloneProperties(cloneFrom);

            Clients = cloneFrom.Clients;
        }

        public override void CloneProperties(HttpClientConfig cloneFrom)
        {
            if (cloneFrom is HttpClientsConfig clientsConfig)
                this.CloneProperties(clientsConfig);
            else
                base.CloneProperties(cloneFrom);
        }
    }
}
