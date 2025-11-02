using AMESA_be.Queue.rabbitmq.Handlers;
using System.Text;
using AMESA_be.Queue.rabbitmq.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AMESA_be.Queue.rabbitmq
{
    public class RpcServer<T> : IHostedService, IDisposable /*IRpcServer*/ where T : IRpcRequestHandler
    {
        private readonly T _handler;
        private readonly ILogger<RpcServer<T>> _logger;

        private readonly RpcConfig _rpcConfig;
        private IModel _channel;

        private IConnection _connection;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RpcServer(ILogger<RpcServer<T>> logger,
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                         IOptionsSnapshot<RpcConfig> rpcConfig,
                         T handler)
        {
            _logger = logger;
            _rpcConfig = rpcConfig.Value;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                try
                {
                    _connection.Close();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "RPC Server shutting down: {Message}", e.Message);
                    throw;
                }

                _connection = null!;
            }
        }

        // public async Task<bool> Start()
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _rpcConfig.RabbitMq.HostAddress,
                    VirtualHost = _rpcConfig.RabbitMq.VirtualHost,
                    UserName = _rpcConfig.RabbitMq.Username,
                    Password = _rpcConfig.RabbitMq.Password
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare("speech-to-text-requests", true,
                false, false, null);
                _channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(_rpcConfig.Queue,
                    false, consumer);
                _logger.LogInformation(" [x] Awaiting RPC requests");
                consumer.Received += (model, ea) =>
                {
                    IRpcResponse response = null!;

                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        RpcRequest? request = JsonConvert.DeserializeObject<RpcRequest>(message);
                        //dynamic convertedFiles = JsonConvert.DeserializeObject(message);
                        //dynamic data = JObject.Parse(message);
                        //convertedFiles = data.message.action.ToString();
                        if (request == null) throw new Exception("Null request");

                        _logger.LogInformation("Receive request. action: {Action}", request.Action);
                        response = _handler.Process(request);
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e, "Handle message error. {Message}", e.Message);
                        response = new RpcResponse
                        {
                            Status = ResponseStatus.Failed.ToString(),
                            StatusMessage = e.Message
                        };
                    }
                    finally
                    {
                        if (response is null)
                        {
                            response = new RpcResponse
                            {
                                Status = ResponseStatus.Failed.ToString(),
                                StatusMessage = "General Error"
                            };
                        }

                        var responseStr = JsonConvert.SerializeObject(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseStr);

                        _logger.LogInformation("Send response. Action: {Action}", response.Status);
                        if (props.ReplyTo != null)
                            _channel.BasicPublish("", props.ReplyTo,
                                replyProps, responseBytes);

                        _channel.BasicAck(ea.DeliveryTag,
                            false);
                    }
                };

                consumer.Shutdown += OnConsumerShutdown!;
                consumer.Registered += OnConsumerRegistered!;
                consumer.Unregistered += OnConsumerUnregistered!;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled!;

                var queueName = _rpcConfig.Queue;

                _channel.BasicConsume(queueName, false, consumer);

                // return await Task.Run(() => _respQueue.Take());
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RabbitMQ connectivity: {ex.Message}");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }
    }
}
