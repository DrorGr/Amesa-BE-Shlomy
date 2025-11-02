using AMESA_be.common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AMESA_be.common.Extensions;
/// <summary>
/// Extensions for Options DI registration
/// </summary>
public static class OptionsRegistrationExtensions
{
    /// <summary>
    /// Adds T class to service collection for dependency injection of IOptions/IOptionsSnapshot
    /// </summary>
    /// <typeparam name="T">Type of configuration class to add as options</typeparam>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionToAdd">Name of configuration section in appsettings.</param>
    /// <param name="keys">
    /// Properties that should be taken offline. 
    /// 
    /// For example: 
    ///     
    ///     InstanceName in CacheConfig
    /// </param>
    public static void AddConfigAsOptions<T>(this IServiceCollection services,
                                             IConfiguration configuration,
                                             string configurationSectionToAdd,
                                             params string[] keys)
        where T : class, IPropertiesCloneable<T>
    {
        services.Configure<T>(o =>
        {
            T config;
            if (configuration.GetValue<bool>("VaultConnection:IsActive"))
            {
                var configAsString = configuration.GetValue<string>(configurationSectionToAdd);
                config = JsonConvert.DeserializeObject<T>(configAsString)!;
            }
            else
            {
                config = configuration.GetSection(configurationSectionToAdd).Get<T>();
            }

            if (config != null)
            {
                if (keys.Length > 0)
                {
                    foreach (var key in keys)
                    {
                        var value = configuration.GetValue<string>($"{configurationSectionToAdd}:{key}");
                        if (!string.IsNullOrEmpty(value))
                        {
                            config.GetType().GetProperty(key)?.SetValue(config, value);
                        }
                    }
                }
                (o as IPropertiesCloneable<T>)!.CloneProperties(config);
            }
        });
    }
}
