using AMESA_be.common.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AMESA_be.common.Extensions
{
    public static class HttpClientConfigExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection sc, IConfiguration configuration)
        {
            HttpClientsConfig config = null;
            var configStr = configuration.GetSection("HttpClients").Get<string>();
            if (!string.IsNullOrEmpty(configStr))
                config = JsonConvert.DeserializeObject<HttpClientsConfig>(configStr);

            if (config == null)
            {
                sc.AddHttpClient();
                return sc;
            }

            sc.AddHttpClient(string.Empty, GetClientConfigurator(null, config));
            if (config.Clients != null)
            {
                foreach (var (name, subConfig) in config.Clients)
                {
                    var fixedName = char.ToUpper(name[0]) + name.Substring(1);
                    sc.AddHttpClient(fixedName, GetClientConfigurator(subConfig, config));
                }
            }

            return sc;
        }

        private static Action<HttpClient> GetClientConfigurator(HttpClientConfig? config, HttpClientConfig defaultConfig)
        {
            return (HttpClient client) =>
            {
                var timeout = config?.TimeoutSec ?? defaultConfig?.TimeoutSec;
                if (timeout != null)
                    client.Timeout = new TimeSpan(0, 0, timeout.Value);
            };
        }
    }
}
