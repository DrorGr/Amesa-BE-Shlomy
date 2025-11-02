using Microsoft.Extensions.Logging;
using RabbitMQ.Client;


namespace AMESA_be.Queue.rabbitmq.Infra
{
    public abstract class RabbitFanoutQueue<T> : RabbitQueue<T>
    {
        protected bool DurableExchange { get; set; } = true;
        protected bool AutoDeleteExchange { get; set; } = false;
        public string ExchangeName { get; protected set; }

        protected RabbitFanoutQueue(IRabbitConnectionBuilder connectionBuilder, ILogger logger, bool isConsumer) : base(connectionBuilder, logger, isConsumer)
        {
            ConsumerAutoAck = true;
        }

        protected override void AddQueueToConnection()
        {
            Channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, DurableExchange, AutoDeleteExchange);
            if (IsConsumer)
            {
                QueueName = $"{ExchangeName}-{Guid.NewGuid():N}";
                Channel.QueueDeclare(QueueName);
                Channel.QueueBind(QueueName, ExchangeName, "");
            }
        }

    }
}
