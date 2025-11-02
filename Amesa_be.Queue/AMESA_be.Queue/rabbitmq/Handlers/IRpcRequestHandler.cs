using AMESA_be.Queue.rabbitmq.Models;

namespace AMESA_be.Queue.rabbitmq.Handlers
{
    public interface IRpcRequestHandler
    {
        public IRpcResponse Process(IRpcRequest request);
    }
}
