namespace AMESA_be.Queue.rabbitmq.Models
{
    public class RpcRequest : IRpcRequest
    {
        public RpcRequest(string action, dynamic parameters)
        {
            Parameters = parameters;
            Action = action;
        }

        public string Action { get; set; }
        public dynamic Parameters { get; set; }
    }
}
