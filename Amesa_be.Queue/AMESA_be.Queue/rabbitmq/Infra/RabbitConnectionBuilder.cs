using System.Net.Security;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AMESA_be.Queue.rabbitmq.Infra
{
    public class RabbitConnectionBuilder : IRabbitConnectionBuilder
    {
        private readonly ILogger<RabbitConnectionBuilder> _logger;
        private readonly RabbitMqSettings? _rabbitConf;

        public RabbitConnectionBuilder(ILogger<RabbitConnectionBuilder> logger, IOptions<RpcConfig> rpcConfig)
        {
            _logger = logger;
            try
            {
                _rabbitConf = rpcConfig.Value?.RabbitMq;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load rpc config. rabbit wont be used.");
                _rabbitConf = null;
            }
        }

        public IConnection? CreateConnection()
        {
            _logger.LogInformation("Creating rabbitMq connection.");
            if (_rabbitConf == null)
            {
                _logger.LogError("Failed to connect to rabbit, rabbit configuration was not provided.");
                return null;
            }

            var factory = new ConnectionFactory
            {
                HostName = _rabbitConf.HostAddress,
                VirtualHost = _rabbitConf.VirtualHost,
            };

            if (_rabbitConf.UseSSL)
            {
                factory.Port = _rabbitConf.Port > 0 ? _rabbitConf.Port : 5671;
                factory.Ssl.Enabled = true;

                // Use modern, secure protocols
                factory.Ssl.Version = SslProtocols.Tls12 | SslProtocols.Tls13;

                factory.Ssl.ServerName = _rabbitConf.ServerName; // Must match the certificate
                factory.Ssl.CertPath = _rabbitConf.PfxFilePath;
                factory.Ssl.CertPassphrase = _rabbitConf.PfxPassword;

                // In production, this line should be removed entirely.
                // For development with self-signed certs, be extremely cautious.
                factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.None;

                if (_rabbitConf.UseCertIdentity)
                    factory.AuthMechanisms = new List<IAuthMechanismFactory> { new ExternalMechanismFactory() };
            }

            try
            {
                var connection = factory.CreateConnection();
                _logger.LogInformation("created rabbitMq connection.");
                return connection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to connect to rabbit: {Msg}", e.Message);
                throw;
            }
        }

    }
}
