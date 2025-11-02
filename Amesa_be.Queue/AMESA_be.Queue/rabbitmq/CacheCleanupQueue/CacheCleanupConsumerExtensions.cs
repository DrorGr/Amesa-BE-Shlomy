using AMESA_be.Queue.rabbitmq.Infra;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AMESA_be.Queue.rabbitmq.CacheCleanupQueue
{
    public static class CacheCleanupConsumerExtensions
    {
        /// <summary>
        /// Adds Cache cleanup consumer to DI.
        /// note that app.StartCacheCleanUpConsumer() must be called to start rabbit consumer on service startup.
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCleanupMessageConsumer(this IServiceCollection sc, IConfiguration configuration)
        {
            sc.AddRabbitConnectionBuilder(configuration);
            sc.AddSingleton<ICacheCleanupConsumer, CacheCleanupConsumer>();
            return sc;
        }

        public static void StartCacheCleanUpConsumer(this IApplicationBuilder app)
        {
            var consumer = app.ApplicationServices.GetService<ICacheCleanupConsumer>();
            if (consumer == null)
            {
                Log.Error("Failed to start cache cleanup consumer, it was not registered.");
            }
        }
    }
}
