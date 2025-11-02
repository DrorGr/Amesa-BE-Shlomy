using AMESA_be.Queue.rabbitmq.Infra;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Events;

namespace AMESA_be.Queue.rabbitmq.CacheCleanupQueue
{
    public sealed class CacheCleanupConsumer : RabbitFanoutQueue<CacheCleanupModel>, ICacheCleanupConsumer
    {
        public delegate void CacheCleanupEvent(CacheCleanupModel model);

        public event CacheCleanupEvent CacheCleanupMessageReceived;

        public CacheCleanupConsumer(IRabbitConnectionBuilder connectionBuilder, ILogger<CacheCleanupConsumer> logger) :
            base(connectionBuilder, logger, true)
        {
            ExchangeName = "CacheCleanup";
            DurableExchange = false; //durable has performance impact and cache clear request are not critical in case of rabbit failure
            Connect();
        }

        protected override void Connect()
        {
            var pipe = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions()
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(3),
                MaxDelay = TimeSpan.FromMinutes(3),
                MaxRetryAttempts = int.MaxValue
            }).Build();
            Task.Run(() => pipe.Execute(base.Connect));
        }

        protected override void Consumer_Received(CacheCleanupModel message, object? sender, BasicDeliverEventArgs e)
        {
            CacheCleanupMessageReceived?.Invoke(message);
        }
    }
}
