namespace AMESA_be.Queue.rabbitmq.Models
{
    public interface IRpcResponse
    {
        public string Action { get; }
        public string Status { get; }
        public string StatusMessage { get; }

        public dynamic Data { get; }
    }
}
