using AMESA_be.Queue.rabbitmq.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client.Events;

namespace AMESA_be.Queue.rabbitmq
{
    public class RpcService : IRpcService
    {
        private readonly ILogger<RpcService> _logger;
        private readonly RpcConfig _rpcConfig;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly EventingBasicConsumer _consumer;
        private readonly IBasicProperties _props;
        private readonly string _replyQueueName;
        private readonly BlockingCollection<IRpcResponse> _respQueue = new BlockingCollection<IRpcResponse>();

        public RpcService(ILogger<RpcService> logger, IOptionsSnapshot<RpcConfig> rpcConfig)
        {
            _logger = logger;
            _rpcConfig = rpcConfig.Value;

            var factory = new ConnectionFactory
            {
                HostName = _rpcConfig.RabbitMq.HostAddress,
                VirtualHost = _rpcConfig.RabbitMq.VirtualHost,
                UserName = _rpcConfig.RabbitMq.Username,
                Password = _rpcConfig.RabbitMq.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                IRpcResponse? response = JsonConvert.DeserializeObject<RpcResponse>(message);

                if (ea.BasicProperties.CorrelationId == correlationId && response != null)
                {
                    _respQueue.Add(response);
                }
            };

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);
        }

        public async Task<IRpcResponse> Call(IRpcRequest request)
        {
            var requestStr = JsonConvert.SerializeObject(request);
            _logger.LogInformation("SpeechToText - RpcService : JSON " + requestStr);
            var requestBytes = Encoding.UTF8.GetBytes(requestStr);

            _channel.BasicPublish(
                "",
                _rpcConfig.Queue,
                _props,
                requestBytes);

            return await Task.Run(() => _respQueue.Take());
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}
