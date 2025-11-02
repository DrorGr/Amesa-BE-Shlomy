namespace AMESA_be.Queue.rabbitmq.Models
{
    public interface IRpcRequest
    {
        public string Action { get; }
        public dynamic Parameters { get; }
    }
}
