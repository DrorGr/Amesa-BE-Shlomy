using AMESA_be.common.Contracts.SettingsConfig;
using AMESA_be.common.Interfaces;

namespace AMESA_be.Queue.rabbitmq
{
    public class RpcConfig : MainAppSettingsConfig, IPropertiesCloneable<RpcConfig>
    {
        public RabbitMqSettings RabbitMq { get; set; }
        //Additional supported QUEUE providers can go here

        public string Queue { get; set; }

        public void CloneProperties(RpcConfig cloneFrom)
        {
            RabbitMq = new RabbitMqSettings();
            RabbitMq.HostAddress = cloneFrom.RabbitMq.HostAddress;
            RabbitMq.Port = cloneFrom.RabbitMq.Port;
            RabbitMq.VirtualHost = cloneFrom.RabbitMq.VirtualHost;
            RabbitMq.Username = cloneFrom.RabbitMq.Username;
            RabbitMq.Password = cloneFrom.RabbitMq.Password;
            Queue = cloneFrom.Queue;
            RabbitMq.UseSSL = cloneFrom.RabbitMq.UseSSL;
            RabbitMq.HostAddress = cloneFrom.RabbitMq.HostAddress;
            RabbitMq.PfxFilePath = cloneFrom.RabbitMq.PfxFilePath;
            RabbitMq.PfxPassword = cloneFrom.RabbitMq.PfxPassword;
            RabbitMq.ServerName = cloneFrom.RabbitMq.ServerName;
            RabbitMq.UseCertIdentity = cloneFrom.RabbitMq.UseCertIdentity;
        }
    }

    public class RabbitMqSettings
    {
        public string HostAddress { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public bool UseSSL { get; set; }
        public string ServerName { get; set; }
        public string PfxFilePath { get; set; }
        public string PfxPassword { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseCertIdentity { get; set; }
    }
}
