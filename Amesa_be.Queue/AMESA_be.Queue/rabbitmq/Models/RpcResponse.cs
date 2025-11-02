namespace AMESA_be.Queue.rabbitmq.Models
{
    public class RpcResponse : IRpcResponse
    {
        public string Action { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public dynamic Data { get; set; }
    }
}
