using AMESA_be.LotteryDevice.BL.Interfaces;
using AMESA_be.LotteryDevice.DAL.Interfaces;
using AMESA_BE.LotteryService.DAL.Implementations;
using AMESA_BELotteryDevice.BL.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AMESA_BE.LotteryService.Helpers
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all services required for the Lottery microservice to the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        public static IServiceCollection AddLotteryServices(this IServiceCollection services)
        {
            // Register Business Logic (BL) services
            services.AddScoped<ILotteryManager, LotteryManager>();

            // Register Data Access Layer (DAL) services
            services.AddScoped<ILotteryRepository, LotteryRepository>();

            // The DbContext is typically registered in Program.cs as it requires IConfiguration.
            // This design keeps the configuration concerns separate.

            return services;
        }
    }
}