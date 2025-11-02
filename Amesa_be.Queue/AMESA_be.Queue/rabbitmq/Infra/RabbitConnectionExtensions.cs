using AMESA_be.common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMESA_be.Queue.rabbitmq.Infra
{
    public static class RabbitConnectionExtensions
    {
        public static IServiceCollection AddRabbitConnectionBuilder(this IServiceCollection sc,
            IConfiguration configuration)
        {
            sc.AddConfigAsOptions<RpcConfig>(configuration, nameof(RpcConfig));
            sc.AddSingleton<IRabbitConnectionBuilder, RabbitConnectionBuilder>();
            return sc;
        }
    }
}
