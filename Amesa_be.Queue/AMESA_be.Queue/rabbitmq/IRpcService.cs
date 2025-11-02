using AMESA_be.Queue.rabbitmq.Models;

namespace AMESA_be.Queue.rabbitmq
{
    public interface IRpcService
    {
        public Task<IRpcResponse> Call(IRpcRequest request);
    }
}
