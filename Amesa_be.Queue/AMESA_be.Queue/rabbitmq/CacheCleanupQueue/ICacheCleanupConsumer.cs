namespace AMESA_be.Queue.rabbitmq.CacheCleanupQueue
{
    public interface ICacheCleanupConsumer
    {
        event CacheCleanupConsumer.CacheCleanupEvent CacheCleanupMessageReceived;
    }
}
