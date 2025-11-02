using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace AMESA_be.Queue.rabbitmq.Infra
{
    public abstract class RabbitQueue<T>(IRabbitConnectionBuilder connectionBuilder, ILogger logger, bool isConsumer) :
        RabbitQueue(connectionBuilder, logger, isConsumer)
    {
        protected override void Consumer_Received(string message, object? sender, BasicDeliverEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<T>(message)!;
            Consumer_Received(data, sender, e);
        }

        protected virtual void Consumer_Received(T message, object? sender, BasicDeliverEventArgs e)
        {

        }
    }

    public abstract class RabbitQueue : IDisposable
    {
        protected readonly IRabbitConnectionBuilder ConnectionBuilder;
        protected IConnection? Connection { get; private set; }
        protected IModel? Channel { get; private set; }
        protected ILogger Logger { get; private set; }
        public string QueueName { get; protected set; }
        protected bool IsConsumer { get; private init; }
        protected bool ConsumerAutoAck { get; set; } = false;
        protected EventingBasicConsumer? Consumer { get; set; }

        protected RabbitQueue(IRabbitConnectionBuilder connectionBuilder, ILogger logger, bool isConsumer)
        {
            ConnectionBuilder = connectionBuilder;
            Logger = logger;
            IsConsumer = isConsumer;
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Channel?.Close();
                Connection?.Close();
                Channel?.Dispose();
                Connection?.Dispose();
            }
        }

        protected virtual void Connect()
        {
            try
            {
                Connection = ConnectionBuilder.CreateConnection();
                if (Connection == null)
                    return;
                Channel = Connection.CreateModel();
                AddQueueToConnection();
                if (IsConsumer)
                {
                    CreateConsumer();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to connect to queue \"{Queue}\": {Msg}", QueueName, e.Message);
                throw;
            }
        }

        protected virtual void CreateConsumer()
        {
            Consumer = new EventingBasicConsumer(Channel);
            Consumer.Received += Internal_Consumer_Received;
            Channel.BasicConsume(QueueName, ConsumerAutoAck, Consumer);
        }

        protected abstract void AddQueueToConnection();

        protected void PublishMessage(ReadOnlyMemory<byte> message, IBasicProperties props = null)
        {
            Logger.LogDebug("trying to send message to queue: {Name}", QueueName);
            if (Channel == null)
            {
                Logger.LogError("Failed to send message to queue {Name}: not connected to rabbit", QueueName);
                return;
            }

            try
            {
                Channel.BasicPublish(QueueName, "", props, message);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to send message to queue {Name}: {Msg}", QueueName, e.Message);
                throw;
            }

        }

        protected void PublishMessage(string message, IBasicProperties props = null)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            PublishMessage(bytes, props);
        }

        protected void PublishMessage(object message, IBasicProperties props = null)
        {
            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            PublishMessage(json, props);
        }

        private void Internal_Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            Logger.LogDebug("Received message from {Name}.", QueueName);
            Consumer_Received(sender, e);
            Logger.LogDebug("finished handling message from {Name}.", QueueName);
        }

        protected virtual void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var str = Encoding.UTF8.GetString(e.Body.Span);
            Consumer_Received(str, sender, e);
        }

        protected virtual void Consumer_Received(string message, object? sender, BasicDeliverEventArgs e) { }
    }
}
