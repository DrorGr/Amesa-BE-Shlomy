namespace AMESA_be.Queue.rabbitmq.CacheCleanupQueue
{
    public class CacheCleanupModel
    {
        public string? Regex { get; set; }
        public bool CleanAll => Regex == null;
    }
}
