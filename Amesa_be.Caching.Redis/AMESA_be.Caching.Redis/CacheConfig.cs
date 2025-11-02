using AMESA_be.common.Interfaces;
using AMESA_be.common.Contracts.SettingsConfig;

namespace AMESA_be.Caching.Redis
{
    public class CacheConfig : MainAppSettingsConfig, IPropertiesCloneable<CacheConfig>
    {
        public static string ConfigName = "CacheConfig";
        public TimeSpan DefaultExpirationTime { get; set; }
        public TimeSpan DefaultShortExpirationTime { get; set; }
        public string InstanceName { get; set; }
        public string GlobalInstance { get; set; }

        private string _redisConnection;
        public string RedisConnection
        {
            get { return _redisConnection; }
            set
            {
                try
                {
                    _redisConnection = value;
                    if (string.IsNullOrEmpty(_redisConnection))
                    {
                        _redisConnection = value;
                    }
                }
                catch (Exception)
                {
                    _redisConnection = value;
                }
            }
        }

        public void CloneProperties(CacheConfig cloneFrom)
        {
            DefaultExpirationTime = cloneFrom.DefaultExpirationTime;
            DefaultShortExpirationTime = cloneFrom.DefaultShortExpirationTime;
            UseHealthCheck = cloneFrom.UseHealthCheck;
            GlobalInstance = cloneFrom.GlobalInstance;
            InstanceName = cloneFrom.InstanceName;
            _redisConnection = cloneFrom._redisConnection;
            RedisConnection = cloneFrom.RedisConnection;
        }
    }
}
